﻿#region Namespaces

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace SmppClient.Core
{
    /// <summary> User Data Header Collection </summary>
    public class UserDataHeaderCollection : List<UserDataHeader>
    {
        #region Public Properties

        /// <summary> Provides array type access </summary>
        /// <param name="iei"></param>
        /// <returns> A UserDataHeader object </returns>
        public UserDataHeader this[InformationElementIdentifiers iei]
        {
            get { return this.FirstOrDefault(k => k.IEI == iei); }
        }

        #endregion

        #region Operator Methods

        /// <summary> Allows the byte array to be assigned to a UserDataHeaderCollection object </summary>
        /// <param name="bytes"></param>
        /// <returns> UserDataHeaderCollection </returns>
        public static implicit operator UserDataHeaderCollection(byte[] bytes)
        {
            var col = new UserDataHeaderCollection();

            var _user_data = new SmppBuffer(DataCodings.Default,
                bytes);
            var offs = 0;

            var udhLength = _user_data.ExtractByte(ref offs);
            var curOffset = offs;

            while (curOffset + udhLength > offs)
            {
                var udhiType = _user_data.ExtractByte(ref offs);
                var udhiLength = _user_data.ExtractByte(ref offs);
                var data = _user_data.ExtractByteArray(ref offs,
                    udhiLength);
                col.Add(UserDataHeader.Create(udhiType,
                    udhiLength,
                    data));
            }

            return col;
        }

        #endregion

        #region Public Methods

        /// <summary> Concatenates the specify message </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="msgRef"></param>
        /// <param name="total"></param>
        /// <param name="seqNum"></param>
        public void AddConcatenatedShortMessages8bit(DataCodings defaultEncoding,
            byte msgRef,
            byte total,
            byte seqNum)
        {
            var tmpBuff = new SmppBuffer(defaultEncoding);
            tmpBuff.AddByte(msgRef);
            tmpBuff.AddByte(total);
            tmpBuff.AddByte(seqNum);

            Add(UserDataHeader.Create(InformationElementIdentifiers.ConcatenatedShortMessages8bit,
                Convert.ToByte(tmpBuff.Length),
                tmpBuff.Buffer));
        }

        /// <summary> Adds data to the user data header list </summary>
        /// <param name="iei"></param>
        /// <param name="data"></param>
        public void Add(InformationElementIdentifiers iei,
            byte[] data)
        {
            Add(UserDataHeader.Create(iei,
                Convert.ToByte(data.Length),
                data));
        }

        #endregion
    }
}