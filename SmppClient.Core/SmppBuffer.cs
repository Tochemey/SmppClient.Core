#region Namespaces

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace SmppClient.Core
{
    /// <summary> The core smpp data structure </summary>
    public class SmppBuffer
    {
        #region Static Public Properties

        /// <summary> ISO-8859-1 Encoding object </summary>
        public static Encoding ISO88591 = Encoding.GetEncoding("ISO-8859-1");

        #endregion

        #region Private Properties

        /// <summary> Buffer of raw bytes </summary>
        private List<byte> ListBuffer { get; }

        #endregion

        #region Peek Methods

        /// <summary> Creates a new Header object from the buffer </summary>
        /// <param name="offset"></param>
        /// <returns> Header </returns>
        public Header PeekHeader(int offset)
        {
            var header = new Header(DefaultEncoding);

            try
            {
                header.Length = ExtractInt(ref offset);
                header.Command = (CommandSet) ExtractInt(ref offset);
                header.Status = (CommandStatus) ExtractInt(ref offset);
                header.Sequence = ExtractInt(ref offset);
            }

            catch { header = null; }

            return header;
        }

        #endregion

        #region Public Properties

        /// <summary> Returns the buffer as bytes </summary>
        public byte[] Buffer => ListBuffer.ToArray();

        /// <summary> The number of bytes we have stored </summary>
        public int Length => ListBuffer.Count;

        /// <summary> Defines the default encoding to use for DataCodings.Default </summary>
        public DataCodings DefaultEncoding = DataCodings.Latin1;

        #endregion

        #region Constructor

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        public SmppBuffer(DataCodings defaultEncoding)
        {
            DefaultEncoding = defaultEncoding;

            ListBuffer = new List<byte>();
        }

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="listBuffer"></param>
        public SmppBuffer(DataCodings defaultEncoding,
            List<byte> listBuffer)
        {
            DefaultEncoding = defaultEncoding;

            ListBuffer = new List<byte>(listBuffer);
        }

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="buf"></param>
        public SmppBuffer(DataCodings defaultEncoding,
            byte[] buf)
        {
            DefaultEncoding = defaultEncoding;

            ListBuffer = new List<byte>();
            ListBuffer.AddRange(buf);
        }

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public SmppBuffer(DataCodings defaultEncoding,
            byte[] bytes,
            int length)
        {
            DefaultEncoding = defaultEncoding;

            var b = new byte[length];
            Array.Copy(bytes,
                b,
                length);

            ListBuffer = new List<byte>();
            ListBuffer.AddRange(b);
        }

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="header"></param>
        public SmppBuffer(DataCodings defaultEncoding,
            Header header)
        {
            DefaultEncoding = defaultEncoding;

            ListBuffer = new List<byte>();

            AddUInt((uint) header.Command);
            AddUInt((uint) header.Status);
            AddUInt(header.Sequence);

            // Use AddFinalLength to add the length to the
            // beginning of the buffer
        }

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="value"></param>
        /// <param name="dataCoding"></param>
        public SmppBuffer(DataCodings defaultEncoding,
            string value,
            DataCodings dataCoding)
        {
            DefaultEncoding = defaultEncoding;

            ListBuffer = new List<byte>();

            AddEncodedString(value,
                dataCoding);
        }

        #endregion

        #region Add Methods

        /// <summary> Called to add the final length to the buffer </summary>
        public void AddFinalLength()
        {
            var b = BitConverter.GetBytes(Length + 4);

            if (BitConverter.IsLittleEndian)
            {
                var tmp = b[0];
                b[0] = b[3];
                b[3] = tmp;

                tmp = b[1];
                b[1] = b[2];
                b[2] = tmp;
            }

            ListBuffer.InsertRange(0,
                b);
        }

        /// <summary> Called to add a byte to the buffer </summary>
        /// <param name="byt"></param>
        public void AddByte(byte byt)
        {
            ListBuffer.Add(byt);
        }

        /// <summary> Called to add an array of bytes to the buffer </summary>
        /// <param name="bytes"></param>
        public void AddBytes(byte[] bytes)
        {
            ListBuffer.AddRange(bytes);
        }

        /// <summary> Called to add a specified number of bytes to the buffer </summary>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public void AddBytes(byte[] bytes,
            int length)
        {
            var b = new byte[length];
            Array.Copy(bytes,
                b,
                length);

            ListBuffer.AddRange(b);
        }

        /// <summary> Called to add an unsigned int to the buffer </summary>
        /// <param name="value"></param>
        public void AddUInt(uint value)
        {
            var b = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian)
            {
                var tmp = b[0];
                b[0] = b[3];
                b[3] = tmp;

                tmp = b[1];
                b[1] = b[2];
                b[2] = tmp;
            }

            ListBuffer.AddRange(b);
        }

        /// <summary> Called to add an integer to the buffer </summary>
        /// <param name="value"></param>
        public void AddInt(int value)
        {
            var b = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian)
            {
                var tmp = b[0];
                b[0] = b[3];
                b[3] = tmp;

                tmp = b[1];
                b[1] = b[2];
                b[2] = tmp;
            }

            ListBuffer.AddRange(b);
        }

        /// <summary> Called to add a short to the buffer </summary>
        /// <param name="value"></param>
        public void AddShort(ushort value)
        {
            var b = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian)
            {
                var tmp = b[0];
                b[0] = b[1];
                b[1] = tmp;
            }

            ListBuffer.AddRange(b);
        }

        /// <summary> Called to add a CString to the buffer </summary>
        /// <param name="value"></param>
        public void AddCString(string value)
        {
            if (value == null) value = string.Empty;

            value = string.Format("{0}{1}",
                value,
                Convert.ToChar(0));
            ListBuffer.AddRange(Encoding.ASCII.GetBytes(value));
        }

        /// <summary> Called to add a Time string to the buffer </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        public void AddTimeString(string value,
            int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException("Len",
                    "Must to be greater than zero");

            var valueBuffer = new StringBuilder(value);

            if (value == null || value.Length == 0) { valueBuffer.Append(Convert.ToChar(0)); }
            else
            {
                if (value.Length < length)
                {
                    for (var i = value.Length; i < length - 1; i = i + 1) valueBuffer.Append(Convert.ToChar(0));

                    valueBuffer.Append(Convert.ToChar(0));
                }
                else
                {
                    throw new ArgumentOutOfRangeException("value",
                        "Too long");
                }
            }

            ListBuffer.AddRange(Encoding.ASCII.GetBytes(valueBuffer.ToString()));
        }

        /// <summary> Called to add the message to the buffer </summary>
        /// <param name="value"></param>
        /// <param name="dataCoding"></param>
        public void AddEncodedString(string value,
            DataCodings dataCoding)
        {
            if (value != null && value.Length > 0)
            {
                if (dataCoding == DataCodings.Default) dataCoding = DefaultEncoding;

                switch (dataCoding)
                {
                    case DataCodings.UCS2:
                    case DataCodings.UnicodeFlashSMS:
                        ListBuffer.AddRange(Encoding.BigEndianUnicode.GetBytes(value));
                        break;

                    case DataCodings.OctetUnspecified:
                    case DataCodings.Octets:
                        ListBuffer.AddRange(Encoding.UTF8.GetBytes(value));
                        break;

                    case DataCodings.Latin1:
                    case DataCodings.Latin1Escape:
                        ListBuffer.AddRange(ISO88591.GetBytes(value));
                        break;

                    case DataCodings.ASCII:
                        value = StringToIA5GSM(value);
                        ListBuffer.AddRange(Encoding.ASCII.GetBytes(value));
                        break;
                }
            }
        }

        /// <summary> Called to add the buffer of another SmppBuffer </summary>
        /// <param name="smppBuffer"></param>
        public void AddSmppBuffer(SmppBuffer smppBuffer)
        {
            ListBuffer.AddRange(smppBuffer.Buffer);
        }

        /// <summary> Called to add a user data object to the buffer </summary>
        /// <param name="userData"></param>
        public void AddUserData(UserData userData)
        {
            var tmpBuff = new SmppBuffer(DefaultEncoding);

            if (userData.Headers.Count > 0) tmpBuff.AddUserDataHeaderCollection(userData.Headers);

            if (userData.ShortMessage != null && userData.ShortMessage.Length > 0) tmpBuff.AddBytes(userData.ShortMessage);

            AddByte(Convert.ToByte(tmpBuff.Length));
            AddSmppBuffer(tmpBuff);
        }

        /// <summary> Called to add a user data header object to the buffer </summary>
        /// <param name="userDataHeader"></param>
        public void AddUserDataHeader(UserDataHeader userDataHeader)
        {
            AddByte(Convert.ToByte(userDataHeader.IEI));
            AddByte(userDataHeader.Length);
            AddBytes(userDataHeader.Data);
        }

        /// <summary> Called to add a user data header collection to the buffer </summary>
        /// <param name="userDataHeaderCollection"></param>
        public void AddUserDataHeaderCollection(UserDataHeaderCollection userDataHeaderCollection)
        {
            if (userDataHeaderCollection.Count > 0)
            {
                var tmpBuff = new SmppBuffer(DefaultEncoding);

                foreach (var header in userDataHeaderCollection) tmpBuff.AddUserDataHeader(header);

                AddByte(Convert.ToByte(tmpBuff.Length));
                AddSmppBuffer(tmpBuff);
            }
        }

        /// <summary> Called to add a TLV object to the buffer </summary>
        /// <param name="tlv"></param>
        public void AddTLV(TLV tlv)
        {
            AddShort(tlv.Tag);
            AddShort(tlv.Length);
            AddBytes(tlv.Value);
        }

        /// <summary> Called to add a TLV Collection to the buffer </summary>
        /// <param name="tlvCollection"></param>
        public void AddTLVCollection(TLVCollection tlvCollection)
        {
            if (tlvCollection.Count > 0) foreach (var tlv in tlvCollection) if (tlv.Length > 0) AddTLV(tlv);
        }

        /// <summary> Called to add a header to the buffer </summary>
        /// <param name="header"></param>
        public void AddHeader(Header header)
        {
            AddUInt((uint) header.Command);
            AddUInt((uint) header.Status);
            AddUInt(header.Sequence);
        }

        /// <summary> Called to add a destination address to the buffer </summary>
        /// <param name="destinationAddress"></param>
        public void AddDestinationAddress(DestinationAddress destinationAddress)
        {
            AddByte((byte) destinationAddress.DestType);
            AddByte(destinationAddress.DestTon);
            AddByte(destinationAddress.DestNpi);
            AddCString(destinationAddress.DestAddr);
        }

        /// <summary> Called to add a list of destination addresses to the buffer </summary>
        /// <param name="destinationAddresses"></param>
        public void AddDestinationAddresses(List<DestinationAddress> destinationAddresses)
        {
            AddByte(Convert.ToByte(destinationAddresses.Count));

            foreach (var destinationAddress in destinationAddresses) AddDestinationAddress(destinationAddress);
        }

        /// <summary> Called to add a unsuccess destination address to the buffer </summary>
        /// <param name="unsuccessDestinationAddress"></param>
        public void AddUnsuccessDestinationAddress(UnsuccessDestinationAddress unsuccessDestinationAddress)
        {
            AddByte(unsuccessDestinationAddress.DestTon);
            AddByte(unsuccessDestinationAddress.DestNpi);
            AddCString(unsuccessDestinationAddress.DestAddr);
            AddUInt((uint) unsuccessDestinationAddress.ErrorStatus);
        }

        /// <summary> Called to add a list of unsuccess destination addresses to the buffer </summary>
        /// <param name="unsuccessDestinationAddresses"></param>
        public void AddUnsuccessDestinationAddresses(List<UnsuccessDestinationAddress> unsuccessDestinationAddresses)
        {
            AddByte(Convert.ToByte(unsuccessDestinationAddresses.Count));

            foreach (var unsuccessDestinationAddress in unsuccessDestinationAddresses) AddUnsuccessDestinationAddress(unsuccessDestinationAddress);
        }

        #endregion

        #region Extract Methods

        /// <summary> Extracts a specified portion of the smpp buffer to an unsigned integer </summary>
        /// <param name="offset"></param>
        /// <returns> A signed integer </returns>
        public uint ExtractInt(ref int offset)
        {
            var tmp = new byte[4];

            if (BitConverter.IsLittleEndian)
            {
                tmp[0] = ListBuffer[3 + offset];
                tmp[1] = ListBuffer[2 + offset];
                tmp[2] = ListBuffer[1 + offset];
                tmp[3] = ListBuffer[0 + offset];
            }
            else
            {
                tmp[0] = ListBuffer[0 + offset];
                tmp[1] = ListBuffer[1 + offset];
                tmp[2] = ListBuffer[2 + offset];
                tmp[3] = ListBuffer[3 + offset];
            }

            offset += 4;

            return BitConverter.ToUInt32(tmp,
                0);
        }

        /// <summary> Extracts a specified portion of the smpp buffer to an unsigned short </summary>
        /// <param name="offset"></param>
        /// <returns> An unsigned short </returns>
        public ushort ExtractShort(ref int offset)
        {
            var tmp = new byte[2];

            if (BitConverter.IsLittleEndian)
            {
                tmp[1] = ListBuffer[0 + offset];
                tmp[0] = ListBuffer[1 + offset];
            }
            else
            {
                tmp[0] = ListBuffer[0 + offset];
                tmp[1] = ListBuffer[1 + offset];
            }

            offset += 2;

            return BitConverter.ToUInt16(tmp,
                0);
        }

        /// <summary> Extracts a specified portion of the smpp buffer to a byte </summary>
        /// <param name="offset"></param>
        /// <returns> A byte </returns>
        public byte ExtractByte(ref int offset)
        {
            var byt = ListBuffer[offset];
            offset += 1;

            return byt;
        }

        /// <summary> Extracts a specified portion of the smpp buffer to a byte array </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns> A byte array </returns>
        public byte[] ExtractByteArray(ref int offset,
            ushort length)
        {
            var byteArray = ListBuffer.GetRange(offset,
                length).ToArray();
            offset = offset + length;

            return byteArray;
        }

        /// <summary> Extracts a specified portion of the smpp buffer to a string </summary>
        /// <param name="offset"></param>
        /// <returns> A string </returns>
        public string ExtractCString(ref int offset)
        {
            var endofStringPosition = offset;

            try
            {
                endofStringPosition = ListBuffer.IndexOf(0,
                    offset);

                if (endofStringPosition > offset)
                {
                    var buffer = ListBuffer.GetRange(offset,
                        endofStringPosition - offset).ToArray();
                    var cString = Encoding.ASCII.GetString(buffer);
                    offset = endofStringPosition + 1;

                    return cString;
                }
                offset += 1;

                return string.Empty;
            }

            catch (Exception)
            {
                offset = endofStringPosition + 1;

                return string.Empty;
            }
        }

        /// <summary> Extracts a specified portion of the smpp buffer to a string </summary>
        /// <param name="length"></param>
        /// <param name="bCodepage"></param>
        /// <param name="offset"></param>
        /// <returns> string </returns>
        public string ExtractEncodedString(int length,
            byte bCodepage,
            ref int offset)
        {
            try
            {
                if (length > 0)
                {
                    var encodedString = string.Empty;

                    if (bCodepage == (byte) DataCodings.Default) bCodepage = (byte) DefaultEncoding;

                    var buffer = ListBuffer.GetRange(offset,
                        length).ToArray();

                    switch (bCodepage)
                    {
                        case (byte) DataCodings.UCS2:
                        case (byte) DataCodings.UnicodeFlashSMS:
                            encodedString = Encoding.BigEndianUnicode.GetString(buffer);
                            break;

                        case (byte) DataCodings.OctetUnspecified:
                        case (byte) DataCodings.Octets:
                            encodedString = Encoding.UTF8.GetString(buffer);
                            break;

                        case (byte) DataCodings.Latin1:
                        case (byte) DataCodings.Latin1Escape:
                            encodedString = ISO88591.GetString(buffer);
                            break;

                        case (byte) DataCodings.ASCII:
                            encodedString = IA5GSMToString(buffer,
                                0,
                                length);
                            break;
                    }

                    offset += length;

                    return encodedString;
                }
                return string.Empty;
            }

            catch
            {
                offset += 1;

                return string.Empty;
            }
        }

        /// <summary> Extracts the entire smpp buffer to a string </summary>
        /// <param name="dataCoding"></param>
        /// <returns> string </returns>
        public string ExtractEncodedString(DataCodings dataCoding)
        {
            try
            {
                // This creates a byte array from the byte list
                // Buffer creates a byte[] so do this just once
                var buffer = Buffer;

                if (buffer.Length > 0)
                {
                    var encodedString = string.Empty;

                    if (dataCoding == DataCodings.Default) dataCoding = DefaultEncoding;

                    switch (dataCoding)
                    {
                        case DataCodings.UCS2:
                        case DataCodings.UnicodeFlashSMS:
                            encodedString = Encoding.BigEndianUnicode.GetString(buffer,
                                0,
                                buffer.Length);
                            break;

                        case DataCodings.OctetUnspecified:
                        case DataCodings.Octets:
                            encodedString = Encoding.UTF8.GetString(buffer,
                                0,
                                buffer.Length);
                            break;

                        case DataCodings.Latin1:
                        case DataCodings.Latin1Escape:
                            encodedString = ISO88591.GetString(buffer,
                                0,
                                buffer.Length);
                            break;

                        case DataCodings.ASCII:
                            encodedString = IA5GSMToString(buffer,
                                0,
                                buffer.Length);
                            break;
                    }

                    return encodedString;
                }
                return string.Empty;
            }

            catch (Exception) { return string.Empty; }
        }

        /// <summary> Extracts the header portion of the PDU </summary>
        /// <param name="header"></param>
        /// <param name="offset"></param>
        public void ExtractHeader(Header header,
            ref int offset)
        {
            var startingOffset = offset;

            // Extract the header
            header.Length = ExtractInt(ref offset);
            header.Command = (CommandSet) ExtractInt(ref offset);
            header.Status = (CommandStatus) ExtractInt(ref offset);
            header.Sequence = ExtractInt(ref offset);

            // Extract the entire pdu
            header.PduData = new SmppBuffer(header.DefaultEncoding,
                ListBuffer.GetRange(startingOffset,
                    (int) header.Length));
        }

        /// <summary> Extracts a specified portion of the smpp buffer to a TLV (options) object </summary>
        /// <param name="offset"></param>
        /// <returns> TLV </returns>
        public TLV ExtractTLV(ref int offset)
        {
            var tlv = TLV.Create();

            tlv.Tag = ExtractShort(ref offset);
            tlv.Length = ExtractShort(ref offset);
            tlv.Value = ExtractByteArray(ref offset,
                tlv.Length);

            return tlv;
        }

        /// <summary> Extracts the destination address portion of the smpp buffer </summary>
        /// <param name="offset"></param>
        /// <returns> DestinationAddress </returns>
        public DestinationAddress ExtractDestinationAddress(ref int offset)
        {
            var destinationAddress = DestinationAddress.Create();

            destinationAddress.DestType = (DestinationAddressType) ExtractByte(ref offset);
            destinationAddress.DestTon = ExtractByte(ref offset);
            destinationAddress.DestNpi = ExtractByte(ref offset);
            destinationAddress.DestAddr = ExtractCString(ref offset);

            return destinationAddress;
        }

        /// <summary> Extract the destination address list portion of the smpp buffer </summary>
        /// <param name="offset"></param>
        /// <returns> List DestinationAddress </returns>
        public List<DestinationAddress> ExtractDestinationAddresses(ref int offset)
        {
            var destinationAddresses = new List<DestinationAddress>();

            var numberOfDests = ExtractByte(ref offset);
            for (var i = 0; i < numberOfDests; i++)
            {
                var destinationAddress = ExtractDestinationAddress(ref offset);
                if (destinationAddress != null) destinationAddresses.Add(destinationAddress);
            }

            return destinationAddresses;
        }

        /// <summary> Extracts the unsuccess destination address portion of the smpp buffer </summary>
        /// <param name="offset"></param>
        /// <returns> UnsuccessDestinationAddress </returns>
        public UnsuccessDestinationAddress ExtractUnsuccessDestinationAddress(ref int offset)
        {
            var unsuccessDestinationAddress = UnsuccessDestinationAddress.Create();

            unsuccessDestinationAddress.DestTon = ExtractByte(ref offset);
            unsuccessDestinationAddress.DestNpi = ExtractByte(ref offset);
            unsuccessDestinationAddress.DestAddr = ExtractCString(ref offset);
            unsuccessDestinationAddress.ErrorStatus = (CommandStatus) ExtractInt(ref offset);

            return unsuccessDestinationAddress;
        }

        /// <summary> Extract the unsuccess destination address list portion of the smpp buffer </summary>
        /// <param name="offset"></param>
        /// <returns> List UnsuccessDestinationAddress </returns>
        public List<UnsuccessDestinationAddress> ExtractUnsuccessDestinationAddresses(ref int offset)
        {
            var unsuccessDestinationAddresses = new List<UnsuccessDestinationAddress>();

            var number_of_dests = ExtractByte(ref offset);
            for (var i = 0; i < number_of_dests; i++)
            {
                var unsuccessDestinationAddress = ExtractUnsuccessDestinationAddress(ref offset);
                if (unsuccessDestinationAddress != null) unsuccessDestinationAddresses.Add(unsuccessDestinationAddress);
            }

            return unsuccessDestinationAddresses;
        }

        /// <summary> Called to extract the user data portion of the smpp buffer </summary>
        /// <param name="udhi"></param>
        /// <param name="offset"></param>
        /// <returns> UserData </returns>
        public UserData ExtractUserData(bool udhi,
            int offset)
        {
            var userData = UserData.Create();

            if (udhi)
            {
                var udhLength = ExtractByte(ref offset);
                var curOffset = offset;
                var countedUdhLegth = 0;

                while (curOffset + udhLength > offset)
                {
                    var udhiType = ExtractByte(ref offset);
                    var udhiLength = ExtractByte(ref offset);
                    countedUdhLegth += udhiLength + 2;

                    if (countedUdhLegth > udhLength)
                    {
                        // Wrong UDH found
                        offset = 0;
                        userData.Headers.Clear();
                        break;
                    }

                    var data = ExtractByteArray(ref offset,
                        udhiLength);
                    userData.Headers.Add(UserDataHeader.Create(udhiType,
                        udhiLength,
                        data));
                }
            }

            if (offset == 0) userData.ShortMessage = Buffer;
            else if (Length - offset > 0)
                userData.ShortMessage = ListBuffer.GetRange(offset,
                    Length - offset).ToArray();

            return userData;
        }

        #endregion

        #region Extract Details Methods

        /// <summary> Extracts a specified portion of the smpp buffer to an unsigned integer </summary>
        /// <param name="propertyName"></param>
        /// <param name="offset"></param>
        /// <returns> PduPropertyDetail </returns>
        public PduPropertyDetail ExtractInt(string propertyName,
            ref int offset)
        {
            var detail = new PduPropertyDetail(offset,
                PduDataTypes.UInt,
                propertyName);

            detail.DataBlock = ListBuffer.GetRange(offset,
                4).ToArray();

            var value = detail.DataBlock;

            if (BitConverter.IsLittleEndian)
            {
                value[0] = ListBuffer[3 + offset];
                value[1] = ListBuffer[2 + offset];
                value[2] = ListBuffer[1 + offset];
                value[3] = ListBuffer[0 + offset];
            }
            else
            {
                value[0] = ListBuffer[0 + offset];
                value[1] = ListBuffer[1 + offset];
                value[2] = ListBuffer[2 + offset];
                value[3] = ListBuffer[3 + offset];
            }

            offset += 4;

            detail.ValueUInt = BitConverter.ToUInt32(value,
                0);

            return detail;
        }

        /// <summary> Extracts a specified portion of the smpp buffer to an unsigned short </summary>
        /// <param name="propertyName"></param>
        /// <param name="offset"></param>
        /// <returns> PduPropertyDetail </returns>
        public PduPropertyDetail ExtractShort(string propertyName,
            ref int offset)
        {
            var detail = new PduPropertyDetail(offset,
                PduDataTypes.UShort,
                propertyName);

            detail.DataBlock = ListBuffer.GetRange(offset,
                2).ToArray();

            var value = detail.DataBlock;

            if (BitConverter.IsLittleEndian)
            {
                value[1] = ListBuffer[0 + offset];
                value[0] = ListBuffer[1 + offset];
            }
            else
            {
                value[1] = ListBuffer[0 + offset];
                value[0] = ListBuffer[1 + offset];
            }

            offset += 2;

            detail.ValueUShort = BitConverter.ToUInt16(value,
                0);

            return detail;
        }

        /// <summary> Extracts a specified portion of the smpp buffer to a byte </summary>
        /// <param name="propertyName"></param>
        /// <param name="offset"></param>
        /// <returns> PduPropertyDetail </returns>
        public PduPropertyDetail ExtractByte(string propertyName,
            ref int offset)
        {
            var detail = new PduPropertyDetail(offset,
                PduDataTypes.Byte,
                propertyName);

            detail.DataBlock = new byte[1];

            detail.DataBlock[0] = ListBuffer[offset];

            offset += 1;

            detail.ValueByte = detail.DataBlock[0];

            return detail;
        }

        /// <summary> Extracts a specified portion of the smpp buffer to a byte array </summary>
        /// <param name="propertyName"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns> PduPropertyDetail </returns>
        public PduPropertyDetail ExtractByteArray(string propertyName,
            ref int offset,
            ushort length)
        {
            var detail = new PduPropertyDetail(offset,
                PduDataTypes.ByteArray,
                propertyName);

            detail.DataBlock = ListBuffer.GetRange(offset,
                length).ToArray();
            offset = offset + length;

            return detail;
        }

        /// <summary> Extracts a specified portion of the smpp buffer to a string </summary>
        /// <param name="propertyName"></param>
        /// <param name="offset"></param>
        /// <returns> PduPropertyDetail </returns>
        public PduPropertyDetail ExtractCString(string propertyName,
            ref int offset)
        {
            var detail = new PduPropertyDetail(offset,
                PduDataTypes.CString,
                propertyName);

            var endofStringPosition = offset;

            try
            {
                endofStringPosition = ListBuffer.IndexOf(0,
                    offset);

                if (endofStringPosition > offset)
                {
                    detail.DataBlock = ListBuffer.GetRange(offset,
                        endofStringPosition - offset).ToArray();
                    detail.ValueString = Encoding.ASCII.GetString(detail.DataBlock);
                    offset = endofStringPosition + 1;
                }
                else
                {
                    detail.DataBlock = new byte[1];
                    detail.DataBlock[0] = ListBuffer[offset];
                    detail.ValueString = null;

                    offset += 1;
                }
            }

            catch (Exception)
            {
                detail.DataBlock = new byte[1];
                detail.DataBlock[0] = ListBuffer[offset];
                detail.ValueString = null;

                offset = endofStringPosition + 1;
            }

            return detail;
        }

        /// <summary> Extracts a specified portion of the smpp buffer to a string </summary>
        /// <param name="propertyName"></param>
        /// <param name="length"></param>
        /// <param name="bCodepage"></param>
        /// <param name="offset"></param>
        /// <returns> PduPropertyDetail </returns>
        public PduPropertyDetail ExtractEncodedString(string propertyName,
            int length,
            byte bCodepage,
            ref int offset)
        {
            var detail = new PduPropertyDetail(offset,
                PduDataTypes.EncodedString,
                propertyName);

            try
            {
                if (length > 0)
                {
                    detail.ValueString = null;

                    if (bCodepage == (byte) DataCodings.Default) bCodepage = (byte) DefaultEncoding;

                    detail.DataBlock = ListBuffer.GetRange(offset,
                        length).ToArray();

                    switch (bCodepage)
                    {
                        case (byte) DataCodings.UCS2:
                        case (byte) DataCodings.UnicodeFlashSMS:
                            detail.ValueString = Encoding.BigEndianUnicode.GetString(detail.DataBlock);
                            break;

                        case (byte) DataCodings.OctetUnspecified:
                        case (byte) DataCodings.Octets:
                            detail.ValueString = Encoding.UTF8.GetString(detail.DataBlock);
                            break;

                        case (byte) DataCodings.Latin1:
                        case (byte) DataCodings.Latin1Escape:
                            detail.ValueString = ISO88591.GetString(detail.DataBlock);
                            break;

                        case (byte) DataCodings.ASCII:
                            detail.ValueString = IA5GSMToString(detail.DataBlock,
                                0,
                                length);
                            break;
                    }

                    offset += length;
                }
                else
                {
                    detail.DataBlock = new byte[1];
                    detail.DataBlock[0] = ListBuffer[offset];
                    detail.ValueString = null;
                }
            }

            catch
            {
                detail.DataBlock = new byte[1];
                detail.DataBlock[0] = ListBuffer[offset];
                detail.ValueString = null;

                offset += 1;
            }

            return detail;
        }

        /// <summary> Extracts the header portion of the PDU </summary>
        /// <param name="offset"></param>
        /// <returns> List PduPropertyDetail </returns>
        public List<PduPropertyDetail> ExtractHeaderDetails(ref int offset)
        {
            var details = new List<PduPropertyDetail>();

            details.Add(ExtractInt("Length",
                ref offset));
            details.Add(ExtractInt("CommandSet",
                ref offset));
            details.Add(ExtractInt("CommandStatus",
                ref offset));
            details.Add(ExtractInt("Sequence",
                ref offset));

            return details;
        }

        /// <summary> Extracts a specified portion of the smpp buffer to a TLV (options) object </summary>
        /// <param name="details"></param>
        /// <param name="offset"></param>
        public void ExtractTLV(List<PduPropertyDetail> details,
            ref int offset)
        {
            details.Add(ExtractShort("TLV_Tag",
                ref offset));

            var detail = ExtractShort("TLV_Length",
                ref offset);
            details.Add(detail);

            details.Add(ExtractByteArray("TLV_Value",
                ref offset,
                detail.ValueUShort));
        }

        /// <summary> Extracts the destination address portion of the smpp buffer </summary>
        /// <param name="details"></param>
        /// <param name="offset"></param>
        public void ExtractDestinationAddress(List<PduPropertyDetail> details,
            ref int offset)
        {
            details.Add(ExtractByte("DestAddress_DestType",
                ref offset));
            details.Add(ExtractByte("DestAddress_DestTon",
                ref offset));
            details.Add(ExtractByte("DestAddress_DestNpi",
                ref offset));
            details.Add(ExtractCString("DestAddress_DestAddr",
                ref offset));
        }

        /// <summary> Extract the destination address list portion of the smpp buffer </summary>
        /// <param name="details"></param>
        /// <param name="offset"></param>
        public void ExtractDestinationAddresses(List<PduPropertyDetail> details,
            ref int offset)
        {
            var detail = ExtractByte("DestAddress_NumberOfDests",
                ref offset);
            details.Add(detail);

            var numberOfDests = detail.ValueByte;
            for (var i = 0; i < numberOfDests; i++)
                ExtractDestinationAddress(details,
                    ref offset);
        }

        /// <summary> Extracts the unsuccess destination address portion of the smpp buffer </summary>
        /// <param name="details"></param>
        /// <param name="offset"></param>
        public void ExtractUnsuccessDestinationAddress(List<PduPropertyDetail> details,
            ref int offset)
        {
            details.Add(ExtractByte("UnsuccessDestAddress_DestTon",
                ref offset));
            details.Add(ExtractByte("UnsuccessDestAddress_DestNpi",
                ref offset));
            details.Add(ExtractCString("UnsuccessDestAddress_DestAddr",
                ref offset));
            details.Add(ExtractInt("UnsuccessDestAddress_ErrorStatus",
                ref offset));
        }

        /// <summary> Extract the unsuccess destination address list portion of the smpp buffer </summary>
        /// <param name="details"></param>
        /// <param name="offset"></param>
        public void ExtractUnsuccessDestinationAddresses(List<PduPropertyDetail> details,
            ref int offset)
        {
            var detail = ExtractByte("UnsuccessDestAddress_NumberOfDests",
                ref offset);
            details.Add(detail);

            var numberOfDests = detail.ValueByte;
            for (var i = 0; i < numberOfDests; i++)
                ExtractUnsuccessDestinationAddress(details,
                    ref offset);
        }

        /// <summary> Called to extract the user data portion of the smpp buffer </summary>
        /// <param name="details"></param>
        /// <param name="udhi"></param>
        /// <param name="offset"></param>
        public void ExtractUserData(List<PduPropertyDetail> details,
            bool udhi,
            int offset)
        {
            PduPropertyDetail detail = null;

            if (udhi)
            {
                detail = ExtractByte("UserData_UdhLength",
                    ref offset);
                details.Add(detail);

                var udhLength = detail.ValueByte;
                var curOffset = offset;
                var countedUdhLegth = 0;

                while (curOffset + udhLength > offset)
                {
                    detail = ExtractByte("UserData_UdhiType",
                        ref offset);
                    details.Add(detail);

                    var udhiType = detail.ValueByte;

                    detail = ExtractByte("UserData_UdhiLength",
                        ref offset);
                    details.Add(detail);

                    var udhiLength = detail.ValueByte;
                    countedUdhLegth += udhiLength + 2;

                    if (countedUdhLegth > udhLength) break;

                    details.Add(ExtractByteArray("UserData_Headers",
                        ref offset,
                        udhiLength));
                }
            }

            if (Length - offset > 0)
            {
                detail = new PduPropertyDetail(offset,
                    PduDataTypes.ByteArray,
                    "UserData_Payload");
                detail.DataBlock = ListBuffer.GetRange(offset,
                    Length - offset).ToArray();
                details.Add(detail);
            }
        }

        #endregion

        #region Slip Methods

        /// <summary> Splits the message into max length parts </summary>
        /// <param name="messageBytes"></param>
        /// <param name="maxLength"></param>
        /// <returns> List byte[] </returns>
        public static List<byte[]> SplitMessageOnParts(byte[] messageBytes,
            int maxLength)
        {
            var messageBuffer = new List<byte>(messageBytes);
            var messageParts = new List<byte[]>();

            // While we have bytes to split
            while (messageBuffer.Count > 0)
            {
                // Can we take max length or do we have less
                var getLength = maxLength > messageBuffer.Count ? messageBuffer.Count : maxLength;

                // Get the bytes
                var part = messageBuffer.GetRange(0,
                    getLength);

                // Does this part end in an escape character. If so then
                // Pull back one byte, we can't split on the escape
                if (part[part.Count - 1] == 0x1B)
                {
                    part.RemoveAt(part.Count - 1);
                    getLength--;
                }

                messageParts.Add(part.ToArray());
                messageBuffer.RemoveRange(0,
                    getLength);
            }

            return messageParts;
        }

        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="maxPartLength"></param>
        /// <returns></returns>
        /// This part was taken out of another lib. which had concatenated messages working
        public static List<string> Split(string message,
            int maxPartLength)
        {
            var result = new List<string>();
            for (var i = 0; i < message.Length; i += maxPartLength)
            {
                var chunkSize = i + maxPartLength < message.Length ? maxPartLength : message.Length - i;
                var chunk = new char[chunkSize];
                message.CopyTo(i,
                    chunk,
                    0,
                    chunkSize);
                result.Add(new string(chunk));
            }
            return result;
        }

        #endregion

        #region Type Conversion Methods

        /// <summary>
        ///     Converts a character from the ISO-8859-1 character set
        ///     into the corresponding character of the GSM "7-bit default alphabet"
        ///     character set
        /// </summary>
        /// <param name="c">The character to convert.</param>
        /// <returns> A string containing the converted character </returns>
        /// <remarks>
        ///     A string is returned instead of a character because some characters
        ///     must be escaped, and consist then of two characters instead of one
        /// </remarks>
        private static string CharToIA5GSM(char c)
        {
            byte retval;
            var escape = false;

            switch (c)
            {
                // Characters not listed here are equal to those in the
                // ISO-8859-1 charset OR not present in it.

                case '@':
                    retval = 0;
                    break;
                case '£':
                    retval = 1;
                    break;
                case '$':
                    retval = 2;
                    break;
                case '¥':
                    retval = 3;
                    break;
                case 'è':
                    retval = 4;
                    break;
                case 'é':
                    retval = 5;
                    break;
                case 'ú':
                    retval = 6;
                    break;
                case 'ì':
                    retval = 7;
                    break;
                case 'í':
                    retval = 7;
                    break;
                case 'ò':
                    retval = 8;
                    break;
                case 'ó':
                    retval = 8;
                    break;
                case 'Ç':
                    retval = 9;
                    break;
                case 'Ø':
                    retval = 11;
                    break;
                case 'ø':
                    retval = 12;
                    break;
                case 'Å':
                    retval = 14;
                    break;
                case 'å':
                    retval = 15;
                    break;
                case '_':
                    retval = 17;
                    break;
                case 'Æ':
                    retval = 28;
                    break;
                case 'æ':
                    retval = 29;
                    break;
                case 'ß':
                    retval = 30;
                    break;
                case 'É':
                    retval = 31;
                    break;
                case '¤':
                    retval = 36;
                    break; // 164 in ISO-8859-1
                case '¡':
                    retval = 64;
                    break;
                // 65-90 capital letters
                case 'Ä':
                    retval = 91;
                    break;
                case 'Ö':
                    retval = 92;
                    break;
                case 'Ñ':
                    retval = 93;
                    break;
                case 'Ü':
                    retval = 94;
                    break;
                case '§':
                    retval = 95;
                    break;
                case '¿':
                    retval = 96;
                    break;
                // 97-122 small letters
                case 'ä':
                    retval = 123;
                    break;
                case 'ö':
                    retval = 124;
                    break;
                case 'ñ':
                    retval = 125;
                    break;
                case 'ü':
                    retval = 126;
                    break;
                case 'à':
                    retval = 127;
                    break;
                case 'á':
                    retval = 127;
                    break;

                // extension table
                case '\f':
                    retval = 10;
                    escape = true;
                    break; // form feed, 0x0C
                case '^':
                    retval = 20;
                    escape = true;
                    break;
                case '{':
                    retval = 40;
                    escape = true;
                    break;
                case '}':
                    retval = 41;
                    escape = true;
                    break;
                case '\\':
                    retval = 47;
                    escape = true;
                    break;
                case '[':
                    retval = 60;
                    escape = true;
                    break;
                case '~':
                    retval = 61;
                    escape = true;
                    break;
                case ']':
                    retval = 62;
                    escape = true;
                    break;
                case '|':
                    retval = 64;
                    escape = true;
                    break;
                case '€':
                    retval = 101;
                    escape = true;
                    break; // 164 in ISO-8859-15

                default:
                    retval = (byte) c;
                    break;
            }

            return (escape ? Convert.ToChar(0x1B).ToString() : "") + Convert.ToChar(retval);
        }

        /// <summary>
        ///     Converts a character from the GSM "7-bit default alphabet"
        ///     into the corresponding character of the ISO-8859-1 character set
        ///     character set
        /// </summary>
        /// <param name="byt"></param>
        /// <param name="prevByt"></param>
        /// <returns> A the correspond GSM character is returned </returns>
        private static char IA5GSMToChar(byte byt,
            byte prevByt)
        {
            var retval = (char) 0;

            if (prevByt != 27)
                switch (byt)
                {
                    // Characters not listed here are equal to those in the
                    // ISO-8859-1 charset OR not present in it.

                    case 0:
                        retval = '@';
                        break;
                    case 1:
                        retval = '£';
                        break;
                    case 2:
                        retval = '$';
                        break;
                    case 3:
                        retval = '¥';
                        break;
                    case 4:
                        retval = 'è';
                        break;
                    case 5:
                        retval = 'é';
                        break;
                    case 6:
                        retval = 'ú';
                        break;
                    case 7:
                        retval = 'ì';
                        break;
                    case 8:
                        retval = 'ò';
                        break;
                    case 9:
                        retval = 'Ç';
                        break;
                    case 11:
                        retval = 'Ø';
                        break;
                    case 12:
                        retval = 'ø';
                        break;
                    case 13:
                        retval = 'Å';
                        break;
                    case 14:
                        retval = 'å';
                        break;
                    case 17:
                        retval = '_';
                        break;
                    case 28:
                        retval = 'Æ';
                        break;
                    case 29:
                        retval = 'æ';
                        break;
                    case 30:
                        retval = 'ß';
                        break;
                    case 31:
                        retval = 'É';
                        break;
                    case 36:
                        retval = '¤';
                        break; // 164 in ISO-8859-1
                    case 64:
                        retval = '¡';
                        break;
                    // 65-90 capital letters
                    case 91:
                        retval = 'Ä';
                        break;
                    case 92:
                        retval = 'Ö';
                        break;
                    case 93:
                        retval = 'Ñ';
                        break;
                    case 94:
                        retval = 'Ü';
                        break;
                    case 95:
                        retval = '§';
                        break;
                    case 96:
                        retval = '¿';
                        break;
                    // 97-122 small letters
                    case 123:
                        retval = 'ä';
                        break;
                    case 124:
                        retval = 'ö';
                        break;
                    case 125:
                        retval = 'ñ';
                        break;
                    case 126:
                        retval = 'ü';
                        break;
                    case 127:
                        retval = 'à';
                        break;

                    default:
                        retval = (char) byt;
                        break;
                }
            else
                switch (byt)
                {
                    // extension table
                    case 10:
                        retval = '\f';
                        break; // form feed, 0x0C
                    case 20:
                        retval = '^';
                        break;
                    case 40:
                        retval = '{';
                        break;
                    case 41:
                        retval = '}';
                        break;
                    case 47:
                        retval = '\\';
                        break;
                    case 60:
                        retval = '[';
                        break;
                    case 61:
                        retval = '~';
                        break;
                    case 62:
                        retval = ']';
                        break;
                    case 64:
                        retval = '|';
                        break;
                    case 101:
                        retval = '€';
                        break; // 164 in ISO-8859-15
                }

            return retval;
        }

        /// <summary>
        ///     Converts a string consisting of characters from the ISO-8859-1
        ///     character set into a string of corresponding characters of the
        ///     GSM "7-bit default alphabet" character set
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>The converted string.</returns>
        /// <remarks>
        ///     Note that the converted string does not need to have the same
        ///     length as the original one because some characters may be escaped
        /// </remarks>
        private static string StringToIA5GSM(string s)
        {
            var newString = new StringBuilder();

            for (var i = 0; i < s.Length; i++)
                newString.Append(CharToIA5GSM(s.Substring(i,
                    1)[0]));

            return newString.ToString();
        }

        /// <summary> Converts a GSM "7-bit default alphabet" character set to a string </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <remarks>
        ///     Note that the converted string does not need to have the same
        ///     length as the original one because some characters may be escaped
        /// </remarks>
        private static string IA5GSMToString(byte[] bytes,
            int offset,
            int length)
        {
            var newString = new StringBuilder();

            byte prevByt = 0;

            for (var i = offset; i < length; i++)
            {
                if (bytes[i] != 27)
                    newString.Append(IA5GSMToChar(bytes[i],
                        prevByt));

                prevByt = bytes[i];
            }

            return newString.ToString();
        }

        /// <summary> Convert the specified byte array to an unsigned short </summary>
        /// <param name="buf"></param>
        /// <param name="startIndex"></param>
        /// <returns> An unsigned short </returns>
        public static ushort BytesToShort(byte[] buf,
            int startIndex)
        {
            var tmp = new byte[2];

            if (BitConverter.IsLittleEndian)
            {
                tmp[1] = buf[0 + startIndex];
                tmp[0] = buf[1 + startIndex];
            }
            else
            {
                tmp[0] = buf[0 + startIndex];
                tmp[1] = buf[1 + startIndex];
            }

            return BitConverter.ToUInt16(tmp,
                0);
        }

        /// <summary> Convert a 7 bit string to 8 bit string </summary>
        /// <param name="ListBuffer"></param>
        /// <returns> A string </returns>
        public static string SeptetToOctet(byte[] ListBuffer)
        {
            var outOffset = 0;
            var inOffset = 0;
            byte Bits = 0x7;
            byte Rest = 0;
            var offset = 0;

            try
            {
                var outBuf = new byte[ListBuffer.Length * 8 / 7];
                outBuf[outOffset] = Convert.ToByte(ListBuffer[offset + inOffset] >> 1);
                outOffset = outOffset + 1;
                inOffset = inOffset + 1;

                while (inOffset < ListBuffer.Length)
                {
                    var SignMask = (1 << Bits) - 1;
                    var RestMask = 0xFF ^ SignMask;

                    var Sign = Convert.ToByte(ListBuffer[offset + inOffset] & SignMask);
                    Sign = Convert.ToByte((Sign << (7 - Bits)) | Rest);

                    Rest = Convert.ToByte(ListBuffer[offset + inOffset++] & RestMask);
                    Rest = Convert.ToByte(Rest >> Bits);
                    Bits -= Convert.ToByte(1);
                    outBuf[outOffset++] = Sign;

                    if (Bits == 0x0)
                    {
                        outBuf[outOffset++] = Rest;
                        Bits = 7;
                        Rest = 0;
                    }
                }

                offset += inOffset;
                return Encoding.ASCII.GetString(outBuf);
            }

            catch { return string.Empty; }
        }

        /// <summary> Convert a 7 bit string to 8 bit string </summary>
        /// <param name="length"></param>
        /// <param name="offset"></param>
        /// <returns> A string </returns>
        public string SeptetToOctet(int length,
            ref int offset)
        {
            var outOffset = 0;
            var inOffset = 0;
            byte Bits = 0x7;
            byte Rest = 0;

            if (length < 1) return string.Empty;

            try
            {
                var outBuf = new byte[length * 8 / 7];
                outBuf[outOffset++] = Convert.ToByte(ListBuffer[offset + inOffset++] >> 1);

                while (inOffset < length)
                {
                    var SignMask = (1 << Bits) - 1;
                    var RestMask = 0xFF ^ SignMask;

                    var Sign = Convert.ToByte(ListBuffer[offset + inOffset] & SignMask);
                    Sign = Convert.ToByte((Sign << (7 - Bits)) | Rest);

                    Rest = Convert.ToByte(ListBuffer[offset + inOffset++] & RestMask);
                    Rest = Convert.ToByte(Rest >> Bits);
                    Bits -= Convert.ToByte(1);
                    outBuf[outOffset++] = Sign;

                    if (Bits == 0x0)
                    {
                        outBuf[outOffset++] = Rest;
                        Bits = 7;
                        Rest = 0;
                    }
                }

                offset += inOffset;
                return Encoding.ASCII.GetString(outBuf);
            }

            catch { return string.Empty; }
        }

        #endregion

        #region Misc Methods

        /// <summary> Called to determine if the data coding is support for text messages </summary>
        /// <param name="dataCoding"></param>
        /// <returns> bool </returns>
        public static bool IsSupportedTextDataCoding(DataCodings dataCoding)
        {
            return dataCoding == DataCodings.Default || dataCoding == DataCodings.Latin1 || dataCoding == DataCodings.Latin1Escape || dataCoding == DataCodings.ASCII || dataCoding == DataCodings.UCS2 || dataCoding == DataCodings.Octets || dataCoding == DataCodings.OctetUnspecified || dataCoding == DataCodings.UnicodeFlashSMS;
        }

        /// <summary> Called to break the data into a list of data blocks </summary>
        /// <param name="maxLength"></param>
        /// <returns> List byte[] </returns>
        public List<byte[]> BreakIntoDataBlocks(int maxLength)
        {
            var byteArrayList = new List<byte[]>();

            var offset = 0;
            while (offset < ListBuffer.Count)
            {
                var length = 4096;
                if (offset + 4096 > ListBuffer.Count) length = ListBuffer.Count - offset;

                byteArrayList.Add(ListBuffer.GetRange(offset,
                    length).ToArray());

                offset += length;
            }

            return byteArrayList;
        }

        #endregion
    }
}