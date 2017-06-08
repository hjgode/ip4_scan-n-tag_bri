using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ip4scanNtag
{
    /// <summary>
    ///String:            1J UN 04-997-7473 123456789 ..
    ///EPC-Header 00110100
    /// Bit 0 to 7
    ///Filter 3 Bit       xxx
    /// Bit 8 to 10       100 -> 1JU
    ///                   101 -> 5JU
    ///                   110 -> 6JU
    ///Partition             011
    /// Bit 11 to 13
    ///Lieferant (DUNS) 30 Bit  000000000100000000010000000001
    /// Bit 14 to 43
    ///Packstück-Nr 30 Bit                                    000000000100000000010000000001
    /// Bit 44 to 73
    ///Behälter-ID 22 Bit                                                                   0000000001000000000100
    /// Bit 74 to 95
    ///decimal is 128 bit
    ///UInt32 is 32Bit
    ///                         "10010100000011100000010001100
    ///                                                       101011111010000101010001011001
    ///                                                                                     1110110101110000010011
    ///                                                                                                           000000000000000"
    /// </summary>
    class VWGTLTAG
    {
        public string LastError = "no error";
        private const byte m_EPCHeader = 0x34; //00110100
        private string m_sFilter="NDEF";

        /// <summary>
        /// a binstr with tag data, the order is reversed but easily to access
        /// starts with bit 0 on the left and ends with bit 95 at the end
        /// </summary>
        private string m_BinStrTagData;
        /// <summary>
        /// a hex string with the tag data
        /// </summary>
        //private string m_HexStrTagData = "112233445566778899001122";

        /// <summary>
        /// Initialize m_cTagData[], m_BinStrTagData, m_HexStrTagData with Data
        /// derived from StrHex
        /// </summary>
        private void InitBinTagData(string StrHex)
        {
            InitBinTagData();

            string tBinStr = tools.HexStr2BinStr(StrHex);//"008009ae3d8b0a7d65048e34"
            string test = tools.BinStr2HexStr(tBinStr);  //"008009ae3d8b0a7d65048e34"
            //only apply changes, if HexStr2BinStr is successfull
            if (tBinStr.Length == 96)
                m_BinStrTagData = tBinStr ;
        }

        /// <summary>
        /// Initialize m_cTagData[], m_BinStrTagData, m_HexStrTagData with zero
        /// </summary>
        private void InitBinTagData()
        {
            char[] cTag = new char[96];
            int i;
            for (i = 0; i < 96; i++ )
            {
                cTag[i] = '0';
                i++;
            }
            m_BinStrTagData = tools.CharAr2BinStr(cTag);
            //m_HexStrTagData = tools.BinStr2HexStr(m_BinStrTagData, 12);
            
            m_iBoxID=0;
            m_iItemNumber = 0;
            m_iSupplierID = 0;
            //encode EPC Header
            m_BinStrTagData = "00110100" + m_BinStrTagData.Substring(8);
            //encode Partition
            m_BinStrTagData = m_BinStrTagData.Substring(0, 11) + "011" + m_BinStrTagData.Substring(14);

        }
        
        public string sFilter {
            get {
                string s = m_BinStrTagData.Substring(8,3);
                if (s.Equals("100"))
                    return "1JUN";
                else if (s.Equals("101"))
                    return "1JUN";
                else if (s.Equals("110"))
                    return "1JUN";
                else
                    return "NDEF";
            }
            set {
                string s = value;
                m_sFilter="NDEF";
                if (s.Length > 4)
                {
                    throw new ArgumentOutOfRangeException("sFilter set: " + s, "String exceeds max length of 4");
                }
                if (s.StartsWith("1JUN"))
                    m_BinStrTagData = m_BinStrTagData.Substring(0,8) + "100" + m_BinStrTagData.Substring(11);
                else if (s.StartsWith("5JUN"))
                    m_BinStrTagData = m_BinStrTagData.Substring(0,8) + "101" + m_BinStrTagData.Substring(11);
                else if (s.StartsWith("6JUN"))
                    m_BinStrTagData = m_BinStrTagData.Substring(0,8) + "110" + m_BinStrTagData.Substring(11);
                else
                    m_BinStrTagData = m_BinStrTagData = m_BinStrTagData.Substring(0, 8) + "000" + m_BinStrTagData.Substring(11);
                this.m_sFilter = value; 
            }
        }
        
        private UInt32 m_iBoxID=0;
        /// <summary>
        /// store/retrieve the BoxID (22Bit)
        /// Bit 74 to 95
        /// </summary>
        public UInt32 BoxID
        {
            get { 
                string s = m_BinStrTagData.Substring(74, 22);
                UInt32 u32 = 0; //"303033353032393035343135"
                u32 = tools.BinStr2Uint(s);
                if (u32 > 0x3FFFFF)
                    throw new ArgumentOutOfRangeException("Set BoxID", "BoxID value exceeds 0x3fffff");
                System.Diagnostics.Debug.WriteLine("Binary BoxID:\n" + Convert.ToString(u32, 2)); //"10001"
                m_iBoxID = u32;
                return m_iBoxID;
            }
            set { 
                m_iBoxID=0;
                //calc uint to BinStr
                string s = "";
                s = tools.Uint2BinStr(value, 22); //100dec = "00100110000000000000000000000000"
                if (tools.BinStr2Uint(s) > 0x3FFFFF)
                    throw new ArgumentOutOfRangeException("Get BoxId","value exceeds limit of 0x3fffff");
                //combine the result BinStr with the internal tagBinStr
                m_BinStrTagData = m_BinStrTagData.Substring(0, 74) + s.Substring(0,22);
                m_iBoxID = value; 
            }
        }



        /// <summary>
        /// SupplierID will hold the Supplier Identifikation
        /// bit 14 to 43 (30 Bit)
        /// </summary>
        private UInt32 m_iSupplierID;
        public UInt32 iSupplierID
        {
            get
            {
                m_iSupplierID = 0;
                string s = m_BinStrTagData.Substring(14, 30);
                    UInt32 u32 = 0; //"303033353032393035343135"
                    u32 = tools.BinStr2Uint(s);
                    if (u32 > 0x3FFFFFFF)
                        throw new ArgumentOutOfRangeException("Set SupplierID", "value exceeds limit of 0x3fffffff");
                    System.Diagnostics.Debug.WriteLine("Binary SuppliererID:\n" + Convert.ToString(u32, 2)); //"10001"
                    m_iSupplierID = u32;
                return m_iSupplierID;

            }
            set
            {
                //calc uint to BinStr
                string s = "";
                s = tools.Uint2BinStr(value, 30);
                if (tools.BinStr2Uint(s) > 0x3FFFFFFF) //1073741823
                    throw new ArgumentOutOfRangeException("Get SupplierID", "value exceeds limit of 0x3fffffff");
                //combine the result BinStr with the internal tagBinStr
                m_BinStrTagData = m_BinStrTagData.Substring(0, 14) + s.Substring(0,30) + m_BinStrTagData.Substring(44);
                m_iSupplierID = value;
            }
        }

        /// <summary>
        /// ItemNumber will hold the Item Number
        /// bit 22 to 51 (30 Bit)
        /// </summary>
        private UInt32 m_iItemNumber;
        public UInt32 iItemNumber {
            get
            {
                m_iItemNumber = 0;
                string s = m_BinStrTagData.Substring(44, 30);
                    UInt32 u32 = 0; //"303033353032393035343135"
                    u32 = tools.BinStr2Uint(s);
                    if (u32 > 0x3FFFFFFF)
                        throw new ArgumentOutOfRangeException("Get ItemNumber", "value exceeds limit of 0x3fffffff");
                    System.Diagnostics.Debug.WriteLine("Binary SuppliererID:\n" + Convert.ToString(u32, 2)); //"10001"
                    m_iItemNumber = u32;
                return m_iItemNumber;

            }
            set
            {
                m_iItemNumber = 0;
                //calc uint to BinStr
                string s = "";
                s = tools.Uint2BinStr(value, 30);
                if (tools.BinStr2Uint(s) > 0x3FFFFFFF)
                    throw new ArgumentOutOfRangeException("Set ItemNumber", "value exceeds limit of 0x3fffffff");
                //combine the result BinStr with the internal tagBinStr
                m_BinStrTagData = m_BinStrTagData.Substring(0, 44) + s.Substring(0,30) + m_BinStrTagData.Substring(74);
                m_iItemNumber = value;
            }
        }

        private bool IsValidTag()
        {
            if (m_BinStrTagData.Length != 96)
                return false;
            if (m_BinStrTagData.Substring(0,8).Equals("00110100") & m_BinStrTagData.Substring(11,3).Equals("011") )
                return true;
            else
                return false;
        }

        /// <summary>
        /// Create a new tag data
        /// </summary>
        /// <param name="sHex">the tag data as 12 bytes hex string
        /// ie "3030 3335303239 3035343135"</param>
        public VWGTLTAG(string sHex)
        {
            InitBinTagData();
            if (sHex.Length != 24)
                throw new ArgumentNullException("vwgtltag (sHex)", "shex is not 24 chars");
            else
                InitBinTagData(sHex);

            m_iItemNumber = this.iItemNumber;
            m_iSupplierID = this.iSupplierID;
            m_iBoxID = this.BoxID;
            m_sFilter = this.sFilter;
        }

        public VWGTLTAG()
        {
            InitBinTagData();
        }

        public VWGTLTAG(string sFilter, string sSupplier, string sItemNumber)
        {
            //Create an all 0 BinStr
            InitBinTagData();

            this.sFilter = sFilter;
            this.iItemNumber = UInt32.Parse(sItemNumber);
            this.iSupplierID = UInt32.Parse(sSupplier);
        }

        public VWGTLTAG(string sFilter, string sSupplier, string sItemNumber, string sBoxID)
        {
            //Create an all 0 BinStr
            InitBinTagData();
            //3030 3335303239 3035343135
            //System.Diagnostics.Debug.WriteLine("sTagDate after encode/decode to BitArray: \n" + s1 + "\n count= " + s1.Length);
            
            this.BoxID = UInt32.Parse(sBoxID);
            this.sFilter=sFilter;
            this.iItemNumber = UInt32.Parse(sItemNumber);
            this.iSupplierID = UInt32.Parse(sSupplier);
        }

        /// <summary>
        /// Returns the actual TagData as hex string
        /// </summary>
        /// <returns>hex string of tag data</returns>
        public override string ToString()
        {
            string s="";
            s = this.m_sFilter + " " + 
                this.m_iSupplierID.ToString().PadLeft(9,'0') + " " +
                this.m_iItemNumber.ToString().PadLeft(9, '0') + " " +
                this.m_iBoxID.ToString().PadLeft(7, '0');
            return s;
        }
        public string GetHex()
        {
            //"00000cae3d8b0a7d65048234"
            return tools.BinStr2HexStr(m_BinStrTagData);

        }
    }
}
