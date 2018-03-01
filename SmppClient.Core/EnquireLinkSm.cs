#region Namespaces

using System.Collections.Generic;

#endregion

namespace SmppClient.Core
{
    /// <summary>
    ///     This message can be sent by either the ESME (Extended Short Message Entity) or
    ///     SMSC (Short Message Service Centre) and is used to provide a confidencecheck of the communication
    ///     path between an ESME and an SMSC
    /// </summary>
    public class EnquireLinkSm : Header, IPacket, IPduDetails
    {
        #region Constructor

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        private EnquireLinkSm(DataCodings defaultEncoding) : base(defaultEncoding,
            CommandSet.EnquireLink,
            SequenceGenerator.Counter)
        { }

        #endregion

        #region IPacket Methods

        /// <summary> Called to return the PDU for this type of object </summary>
        /// <returns> byte[] </returns>
        public byte[] GetPDU()
        {
            var tmpBuff = new SmppBuffer(DefaultEncoding,
                this);

            tmpBuff.AddFinalLength();

            return tmpBuff.Buffer;
        }

        #endregion

        #region PDU Detail Methods

        /// <summary> Called to return a list of property details from the PDU </summary>
        /// <returns> List PduPropertyDetail </returns>
        public List<PduPropertyDetail> Details()
        {
            List<PduPropertyDetail> details = null;

            try
            {
                var offset = 0;

                details = PduData.ExtractHeaderDetails(ref offset);
            }

            catch { }

            return details;
        }

        #endregion

        #region Factory Methods

        /// <summary> Called to create a new EnquireLink object </summary>
        /// <param name="defaultEncoding"></param>
        /// <returns> EnquireLink </returns>
        public static EnquireLinkSm Create(DataCodings defaultEncoding)
        {
            return new EnquireLinkSm(defaultEncoding);
        }

        /// <summary> Called to create a new EnquireLink object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <returns> EnquireLink </returns>
        public static EnquireLinkSm Create(DataCodings defaultEncoding,
            SmppBuffer buf,
            ref int offset)
        {
            var enquireLink = new EnquireLinkSm(defaultEncoding);

            try
            {
                buf.ExtractHeader(enquireLink,
                    ref offset);
            }

            catch { enquireLink = null; }

            return enquireLink;
        }

        #endregion
    }
}