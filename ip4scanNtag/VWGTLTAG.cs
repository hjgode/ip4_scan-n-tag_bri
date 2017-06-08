using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ip4scanNtag
{
    //String:            1J UN 04-997-7473 123456789 ..
    //########### B I N A R Y ###############
    //EPC-Header 00110100
    // Bit 0 to 7
    //Filter 3 Bit       xxx
    // Bit 8 to 10       100 -> 1JU
    //                   101 -> 5JU
    //                   110 -> 6JU
    //Partition             011
    // Bit 11 to 13
    //Lieferant (DUNS) 30 Bit  000000000100000000010000000001
    // Bit 14 to 43
    //Packstück-Nr 30 Bit                                    000000000100000000010000000001
    // Bit 44 to 73
    //Behälter-ID 22 Bit                                                                   0000000001000000000100
    // Bit 74 to 95

    //########## S T R I N G ################
    //        s(start, length)
    //Header: s(88,8) "00101100"    // lowest bit left!
    //Filter: s(85,3)
    //Part.:  s(82,3) "110" lowest bit left!
    //Suppl.: s(52,30)
    //ItemNr: s(22,30)
    //BoxID:  s(0,22)

    /// <summary>
    ///                                                                                                           000000000000000"
    /// </summary>
    public class VWGTLTAG
    {
        private const string sBinStrHeader = "00101100";// lowest bit left!
        private const string sBinStrPartition = "110";// lowest bit left!
        private const string sBinStr1JUN = "001";
        private const string sBinStr5JUN = "101";
        private const string sBinStr6JUN = "011";

        public static string LastError = "no error";
        private const byte m_EPCHeader = 0x34; //00110100
        private string m_sFilter="NDEF";

        /// <summary>
        /// a binstr with tag data, the order is reversed but easily to access
        /// starts with bit 0 on the left and ends with bit 95 at the end
        /// </summary>
        private static string m_BinStrTagData;
        /// <summary>
        /// a hex string with the tag data
        /// </summary>
        //private string m_HexStrTagData = "112233445566778899001122";

        /// <summary>
        /// Initialize m_BinStrTagData with Data
        /// derived from StrHex
        /// </summary>
        private void InitBinTagData(string StrHex)
        {
            //"34AC2FA98811D6F345400001"
            //Binary: 001101001010110000101111101010011000100000010001110101101111001101000101010000000000000000000001
            //string: 100001000001000100010100000000010001010111011001100100001101101000011010101000101100100011101100
            //5J UN 04-997-7473 123456789
            InitBinTagData();

            string tBinStr = tools.HexStr2BinStr(StrHex);
            string test = tools.BinStr2HexStr(tBinStr);
            if (!test.Equals(StrHex.ToLower()))
                System.Diagnostics.Debugger.Break();
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
            m_iBoxID=0;
            m_iItemNumber = 0;
            m_iSupplierID = 0;
            //encode EPC Header
            m_BinStrTagData = m_BinStrTagData.Substring(0,88) + sBinStrHeader ;
            //encode Partition
            m_BinStrTagData = m_BinStrTagData.Substring(0, 82) + sBinStrPartition + m_BinStrTagData.Substring(85);

        }
        private static string[] sFilters ={ "1JUN", "5JUN", "6JUN" };
        public string sFilter {
            get {
                string s = m_BinStrTagData.Substring(85,3);
                if (s.Equals(sBinStr1JUN))// lowest bit left!
                    return "1JUN";
                else if (s.Equals(sBinStr5JUN))// lowest bit left!
                    return "5JUN";
                else if (s.Equals(sBinStr6JUN))// lowest bit left!
                    return "6JUN";
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
                    m_BinStrTagData = m_BinStrTagData.Substring(0, 85) + sBinStr1JUN + m_BinStrTagData.Substring(88);
                else if (s.StartsWith("5JUN"))
                    m_BinStrTagData = m_BinStrTagData.Substring(0, 85) + sBinStr5JUN + m_BinStrTagData.Substring(88);
                else if (s.StartsWith("6JUN"))
                    m_BinStrTagData = m_BinStrTagData.Substring(0, 85) + sBinStr6JUN + m_BinStrTagData.Substring(88);
                else
                    m_BinStrTagData = m_BinStrTagData.Substring(0,85) + "000" + m_BinStrTagData.Substring(88);
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
                string s = m_BinStrTagData.Substring(0, 22);
                UInt32 u32 = 0;
                u32 = tools.BinStr2Uint(s);
                if (u32 > 0x3FFFFF)
                    throw new ArgumentOutOfRangeException("Set BoxID", "BoxID value exceeds 0x3fffff");
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
                m_BinStrTagData = s.Substring(0,22) + m_BinStrTagData.Substring(22);
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
                string s = m_BinStrTagData.Substring(52, 30);
                    UInt32 u32 = 0; 
                    u32 = tools.BinStr2Uint(s);
                    if (u32 > 0x3FFFFFFF)
                        throw new ArgumentOutOfRangeException("Set SupplierID", "value exceeds limit of 0x3fffffff");
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
                m_BinStrTagData = m_BinStrTagData.Substring(0, 52) + s.Substring(0,30) + m_BinStrTagData.Substring(82);
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
                string s = m_BinStrTagData.Substring(22, 30);
                    UInt32 u32 = 0;
                    u32 = tools.BinStr2Uint(s);
                    if (u32 > 0x3FFFFFFF)
                        throw new ArgumentOutOfRangeException("Get ItemNumber", "value exceeds limit of 0x3fffffff");
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
                m_BinStrTagData = m_BinStrTagData.Substring(0, 22) + s.Substring(0,30) + m_BinStrTagData.Substring(52);
                m_iItemNumber = value;
            }
        }

        public static bool IsValidTag(string sHex)
        {
            m_BinStrTagData = tools.HexStr2BinStr(sHex);
            if (m_BinStrTagData.Length != 96)
                return false;
            if (m_BinStrTagData.Substring(88,8).Equals(sBinStrHeader) & m_BinStrTagData.Substring(82,3).Equals(sBinStrPartition ) )
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
            try
            {
                this.iItemNumber = UInt32.Parse(sItemNumber);
                this.iSupplierID = UInt32.Parse(sSupplier);
            }
            catch
            {
                LastError = "error in applying numbers";
            }
        }

        public VWGTLTAG(string sFilter, string sSupplier, string sItemNumber, string sBoxID)
        {
            //Create an all 0 BinStr
            InitBinTagData();
            //3030 3335303239 3035343135
            //System.Diagnostics.Debug.WriteLine("sTagDate after encode/decode to BitArray: \n" + s1 + "\n count= " + s1.Length);
            
            this.sFilter=sFilter;
            try
            {
                this.iItemNumber = UInt32.Parse(sItemNumber);
                this.iSupplierID = UInt32.Parse(sSupplier);
                this.BoxID = UInt32.Parse(sBoxID);
            }
            catch
            { 
                LastError = "error in applying numbers";
            }
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
            //"34AC2FA98811D6F345400001"
            //Binary: 001101001010110000101111101010011000100000010001110101101111001101000101010000000000000000000001
            //5J UN 04-997-7473 123456789
            string sHex = tools.BinStr2HexStr(m_BinStrTagData);
            return sHex;

        }
        /// <summary>
        /// Test, if a Barcode is valid
        /// tests for filter and if Supplier and ItemNumber parts can be converted to a valid number
        /// </summary>
        /// <param name="sBarcode">Barcode to check
        /// ie.: 1JUN049977473123456789</param>
        /// <returns></returns>
        public static bool CheckBarCode(string sBarcode)
        {
            if (sBarcode.Length != 22)
            {
                LastError="Barcode wrong Length";
                return false;
            }
            //test for filter
            string sFilter = sBarcode.Substring(0, 4); 
            bool bFilterOK = false;
            for (int i = 0; i < sFilters.Length; i++)
            {
                if (sFilter.Equals(sFilters[i]))
                    bFilterOK = true;
            }
            //test SupplierID
            string sNumber = sBarcode.Substring(4, 9); 
            bool bSupplierID = false;
            UInt32 uNumber = 0;
            try {
                uNumber = UInt32.Parse(sNumber);
                if (uNumber>0x3fffffff)
                {
                    LastError="SupplierID exceeds 0x3FFFFFFF";
                    return false;
                }
                string test = uNumber.ToString().PadLeft(9, '0');
                if (test.Equals(sNumber))
                    bSupplierID = true;
                else
                    bSupplierID = false;
            }
            catch { 
                bSupplierID = false;
            }
            //test ItemNumber
            sNumber = sBarcode.Substring(13, 9);
            bool bItemNumber = false;
            uNumber = 0;
            try {
                uNumber = UInt32.Parse(sNumber);
                if (uNumber>0x3fffffff)
                {
                    LastError="ItemNumber exceeds 0x3FFFFFFF";
                    return false;
                }
                string test = uNumber.ToString().PadLeft(9, '0');
                if (test.Equals(sNumber))
                    bItemNumber = true;
                else
                    bItemNumber = false;
            }
            catch {
                bItemNumber = false;
            }
            if (bFilterOK & bSupplierID & bItemNumber)
                return true;
            else
                return false;
        }

    }
}
