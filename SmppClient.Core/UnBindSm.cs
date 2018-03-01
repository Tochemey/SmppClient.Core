#region Namespaces

using System.Collections.Generic;

#endregion

namespace SmppClient.Core
{
    /// <summary>
    ///     Deregister an instance of an ESME (Extended Short Message Entity) from the
    ///     SMSC (Short Message Service Centre) and inform the SMSC that the ESME no longer wishes
    ///     to use this network connection for the submission or delivery of messages
    /// </summary>
    public class UnBindSm : Header, IPacket, IPduDetails
    {
        #region Constructor

        /// <summary> Constructor </summary>
        /// <param name="defaultEncoding"></param>
        private UnBindSm(DataCodings defaultEncoding) : base(defaultEncoding,
            CommandSet.Unbind,
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

        /// <summary> Called to create a PduUnBind object </summary>
        /// <param name="defaultEncoding"></param>
        /// <returns> PduUnBind </returns>
        public static UnBindSm Create(DataCodings defaultEncoding)
        {
            return new UnBindSm(defaultEncoding);
        }

        /// <summary> Called to create a PduUnBind object </summary>
        /// <param name="defaultEncoding"></param>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <returns> PduUnBind </returns>
        public static UnBindSm Create(DataCodings defaultEncoding,
            SmppBuffer buf,
            ref int offset)
        {
            var unBind = new UnBindSm(defaultEncoding);

            try
            {
                buf.ExtractHeader(unBind,
                    ref offset);
            }

            catch { unBind = null; }

            return unBind;
        }

        #endregion
    }
}