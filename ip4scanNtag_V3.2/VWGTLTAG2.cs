using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ip4scanNtag
{
#region version2 comment
    //VERSION 2
    //String:            1J UN 049977473 123456789 ..
    //########### B I N A R Y ###############
    //EPC-Header 00110100
    // Bit 88 to 95
    //Filter 3 Bit       xxx
    // Bit 85 to 87      zzz
    //Partition             110
    // Bit 82 to 84
    //Box-Ident 20 Bit            00000000010000000001
    // Bit 62 to 81
    // NEW: old Filter is renamed to DataID
    //DataID (Data identifier + Agency code)          yy
    // Bit 60 to 61            00 = 1JUN
    //                         01 = 5JUN
    //                         10 = 6JUN
    //                         11 = NDEF
    //Supplier-ID 30 Bit                                000000000100000000010000000001
    // Bit 30 to 59
    //Item-Number 30 Bit                                                              000000000100000000010000000001
    // Bit 0 to 29

    //"3400000040BEA620475BCD15"
    //Binary: 001101000000000000000000000000000100000010111110101001100010000001000111010110111100110100010101
    //string: 101010001011001111011010111000100000010001100101011111010000001000000000000000000000000000101100
    //1J UN 049977473 123456789
#endregion

    /// <summary>
    ///                                                                                                           000000000000000"
    /// </summary>
    class VWGTLTAG2
    {
        public const string sBinStrEPCHeader = "00101100";// lowest bit left!
        public const string sBinStrPartition = "011";// lowest bit left!
        private const string sBinStr1JUN = "00";// lowest bit left!
        private const string sBinStr5JUN = "10";// lowest bit left!
        private const string sBinStr6JUN = "01";// lowest bit left!

        public static string LastError = "no error";
        private const byte m_EPCHeader = 0x34; //00110100
        private string m_sDataID="NDEF";
        private string m_sFilter="000";

        /// <summary>
        /// a binstr with tag data, the order is reversed but easily to access
        /// starts with bit 0 on the left and ends with bit 95 at the end
        /// </summary>
        private string m_BinStrTagData; //static?
        /// <summary>
        /// a hex string with the tag data
        /// </summary>
        private string m_HexStrTagData = "112233445566778899001122";

        /// <summary>
        /// Initialize m_BinStrTagData with Data
        /// derived from StrHex
        /// </summary>
        private void InitBinTagData(string StrHex)
        {
            InitBinTagData();
            m_HexStrTagData = StrHex;

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
            m_iFilter = 0;
            m_iBOXidentification = 0;
            m_iPackageNumber = 0;
            m_iSupplierID = 0;
            //encode BinStr with default data
            //data is in 0 to 81
            m_BinStrTagData = m_BinStrTagData.Substring(0,82) + sBinStrPartition + m_sFilter + sBinStrEPCHeader ;
            if (m_BinStrTagData.Length != 96)
                System.Diagnostics.Debugger.Break();

        }
        private static string[] sDataIDs ={ "1JUN", "5JUN", "6JUN" };
        public string sDataID {
            get {
                string s = m_BinStrTagData.Substring(60,2);
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
                m_sDataID="NDEF";
                if (s.Length > 4)
                {
                    throw new ArgumentOutOfRangeException("sDataID set: " + s, "String exceeds max length of 4");
                }
                if (s.StartsWith("1JUN"))
                    m_BinStrTagData = m_BinStrTagData.Substring(0, 60) + sBinStr1JUN + m_BinStrTagData.Substring(62);
                else if (s.StartsWith("5JUN"))
                    m_BinStrTagData = m_BinStrTagData.Substring(0, 60) + sBinStr5JUN + m_BinStrTagData.Substring(62);
                else if (s.StartsWith("6JUN"))
                    m_BinStrTagData = m_BinStrTagData.Substring(0, 60) + sBinStr6JUN + m_BinStrTagData.Substring(62);
                else
                    m_BinStrTagData = m_BinStrTagData.Substring(0,60) + "00" + m_BinStrTagData.Substring(62);
                this.m_sDataID = value; 
            }
        }
        
        private UInt32 m_iBOXidentification=0;
        /// <summary>
        /// This is to store a BOX identification
        /// store/retrieve the Filter (22Bit)
        /// Bit 62 to 81
        /// </summary>
        public UInt32 iBOXidentification
        {
            get { 
                string s = m_BinStrTagData.Substring(62, 20);
                UInt32 u32 = 0;
                u32 = tools.BinStr2Uint(s);
                if (u32 > 0xFFFFF)
                    throw new ArgumentOutOfRangeException("Set Filter", "Filter value exceeds 0x3fffff");
                m_iBOXidentification = u32;
                return m_iBOXidentification;
            }
            set { 
                m_iBOXidentification=0;
                //calc uint to BinStr
                string s = "";
                s = tools.Uint2BinStr(value, 20); //100dec = "00100110000000000000000000000000"
                if (tools.BinStr2Uint(s) > 0xFFFFF)                                         
                    throw new ArgumentOutOfRangeException("Get Filter","value exceeds limit of 0x3fffff");
                //combine the result BinStr with the internal tagBinStr
                m_BinStrTagData = m_BinStrTagData.Substring(0,62) +
                                  s.Substring(0,20) + m_BinStrTagData.Substring(82);
                if (m_BinStrTagData.Length != 96)
                    throw new ArgumentOutOfRangeException("iBOXidentification set", "m_BinStrTagData ERROR!");
                m_iBOXidentification = value; 
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
                string s = m_BinStrTagData.Substring(30, 30);
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
                m_BinStrTagData = m_BinStrTagData.Substring(0, 30) + s.Substring(0,30) + m_BinStrTagData.Substring(60);
                m_iSupplierID = value;
            }
        }

        /// <summary>
        /// store/retrive the Filter, 3 bits to distinguish multiple tags at one box
        /// </summary>
        private uint m_iFilter;
        public uint iFilter {
            get 
            {
                //m_iFilter = 0;
                string s = m_BinStrTagData.Substring(85, 3);
                System.Diagnostics.Debug.WriteLine("iFilter: \t" + m_BinStrTagData + "\niFilter= \t" + s);
                UInt32 u32 = 0;
                u32 = tools.BinStr2Uint(s);
                if (u32 > 0x07)
                    throw new ArgumentOutOfRangeException("Get TAG Identification", "value exceeds limit of 0x07");
                m_iFilter = u32;
                return m_iFilter;
            }
            set
            {
                m_iFilter = 0;
                //calc uint to BinStr
                string s = "";
                s = tools.Uint2BinStr(value, 3);
                if (tools.BinStr2Uint(s) > 0x07)
                    throw new ArgumentOutOfRangeException("Set Box Identification", "value exceeds limit of 0x07");
                //combine the result BinStr with the internal tagBinStr
                m_BinStrTagData = m_BinStrTagData.Substring(0, 85) + s.Substring(0, 3) + m_BinStrTagData.Substring(88);
                if (m_BinStrTagData.Length!=96)
                    throw new ArgumentOutOfRangeException("iFilter set", "m_BinStrTagData ERROR!");
                m_iFilter = value;
                System.Diagnostics.Debug.WriteLine("iFilter: \t" + m_BinStrTagData + "\niFilter= \t" + s);
            }
        }
        /// <summary>
        /// PackageNumber will hold the Item Number
        /// bit 22 to 51 (30 Bit)
        /// </summary>
        private UInt32 m_iPackageNumber;
        public UInt32 iPackageNumber {
            get
            {
                m_iPackageNumber = 0;
                string s = m_BinStrTagData.Substring(0, 30);
                    UInt32 u32 = 0;
                    u32 = tools.BinStr2Uint(s);
                    if (u32 > 0x3FFFFFFF)
                        throw new ArgumentOutOfRangeException("Get PackageNumber", "value exceeds limit of 0x3fffffff");
                    m_iPackageNumber = u32;
                return m_iPackageNumber;

            }
            set
            {
                m_iPackageNumber = 0;
                //calc uint to BinStr
                string s = "";
                s = tools.Uint2BinStr(value, 30);
                if (tools.BinStr2Uint(s) > 0x3FFFFFFF)
                    throw new ArgumentOutOfRangeException("Set PackageNumber", "value exceeds limit of 0x3fffffff");
                //combine the result BinStr with the internal tagBinStr
                m_BinStrTagData = s.Substring(0,30) + m_BinStrTagData.Substring(30);
                m_iPackageNumber = value;
            }
        }

        public static bool IsValidTag(string sHex)
        {
            string sBinStrTagData;
            sBinStrTagData = tools.HexStr2BinStr(sHex);
            if (sBinStrTagData.Length != 96)
                return false;
            if (! sBinStrTagData.Substring(88,8).Equals(sBinStrEPCHeader))
                return false;
            if (! sBinStrTagData.Substring(82,3).Equals(sBinStrPartition))
                return false;
            //dont need this, as it can vary
            //if (sBinStrTagData.Substring(60,2).Equals("11"))
            //    return false;
            return true;
        }

        /// <summary>
        /// Create a new tag data
        /// </summary>
        /// <param name="sHex">the tag data as 12 bytes hex string
        /// ie "3400000040BEA620475BCD15"</param>
        public VWGTLTAG2(string sHex)
        {
            InitBinTagData();
            if (sHex.Length != 24)
                throw new ArgumentNullException("vwgtltag (sHex)", "sHex is not 24 chars");
            else
                InitBinTagData(sHex);

            m_iPackageNumber = this.iPackageNumber;
            m_iSupplierID = this.iSupplierID;
            m_iBOXidentification = this.iBOXidentification;
            m_sDataID = this.sDataID;
            m_iFilter = this.iFilter;
        }

        public VWGTLTAG2()
        {
            InitBinTagData();
        }
        /// <summary>
        /// create a new tag
        /// </summary>
        /// <param name="sDataID">DataID="1JUN", "5JUN" or "6JUN"</param>
        /// <param name="sSupplier">the supplier identification, a number less than 0x3fffffff</param>
        /// <param name="sPackageNumber">the package number, a number less than 0x3fffffff</param>
        public VWGTLTAG2(string sDataID, string sSupplier, string sPackageNumber)
        {
            //Create an all 0 BinStr
            InitBinTagData();

            this.sDataID = sDataID;
            try
            {
                this.iPackageNumber = UInt32.Parse(sPackageNumber);
                this.iSupplierID = UInt32.Parse(sSupplier);
            }
            catch
            {
                LastError = "error in applying numbers";
            }
        }

        public VWGTLTAG2(string sDataID, string sSupplier, string sPackageNumber, string sBoxID)
        {
            //Create an all 0 BinStr
            InitBinTagData();
            //System.Diagnostics.Debug.WriteLine("sTagDate after encode/decode to BitArray: \n" + s1 + "\n count= " + s1.Length);
            
            this.sDataID=sDataID;
            try
            {
                this.iPackageNumber = UInt32.Parse(sPackageNumber);
                this.iSupplierID = UInt32.Parse(sSupplier);
                this.iBOXidentification = UInt32.Parse(sBoxID);
            }
            catch
            { 
                LastError = "error in applying numbers";
            }
        }

        public VWGTLTAG2(string sDataID, string sSupplier, string sPackageNumber, string sBoxID, string sFilter)
        {
            //Create an all 0 BinStr
            InitBinTagData();
            //System.Diagnostics.Debug.WriteLine("sTagDate after encode/decode to BitArray: \n" + s1 + "\n count= " + s1.Length);
            
            this.sDataID=sDataID;
            try
            {
                this.iPackageNumber = UInt32.Parse(sPackageNumber);
                this.iSupplierID = UInt32.Parse(sSupplier);
                this.iBOXidentification = UInt32.Parse(sBoxID);
                this.iFilter = UInt32.Parse(sFilter);
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
            s = this.m_sDataID + " " + 
            this.m_iSupplierID.ToString().PadLeft(9, '0') + " " +
            this.m_iPackageNumber.ToString().PadLeft(9, '0') + "[" +
            this.m_iFilter.ToString().PadLeft(2,'0') + "](" +
            this.m_iBOXidentification.ToString().PadLeft(7,'0') + ")";
            //below is NOT part of the readable data
            //this.m_iBOXidentification.ToString().PadLeft(2, '0');
            //this.m_iFilter 
            return s;
        }
        //added 6. feb 2008
        public sbyte[] sByte{
            get 
            {
                string str = HexData;
                sbyte[] bOut=new sbyte[str.Length/2];
                int j = 0;
                for (int i = 0; i < str.Length; i += 2)
                {
                    // den Hex-Wert aus dem langen String holen..
                    string sub = str.Substring(i, 2);
                    // Parse-Methode von byte aufrufen
                    // Parameter1: Der Hex-String
                    // Parameter2: Eine Angabe das es sich dabei um Hex handelt..
                    byte b = byte.Parse(sub, System.Globalization.NumberStyles.HexNumber);
                    bOut[j++] = (sbyte)b;
                    //Console.WriteLine(sub + " -> " + b.ToString());
                }
                return  bOut;
            }
            
        }
        public byte[] Byte
        {
            get
            {
                string str = HexData;
                byte[] bOut = new byte[str.Length / 2];
                int j = 0;
                for (int i = 0; i < str.Length; i += 2)
                {
                    // den Hex-Wert aus dem langen String holen..
                    string sub = str.Substring(i, 2);
                    // Parse-Methode von byte aufrufen
                    // Parameter1: Der Hex-String
                    // Parameter2: Eine Angabe das es sich dabei um Hex handelt..
                    byte b = byte.Parse(sub, System.Globalization.NumberStyles.HexNumber);
                    bOut[j++] = (byte)b;
                    //Console.WriteLine(sub + " -> " + b.ToString());
                }
                return bOut;
            }

        }

        public string HexData
        {
            get 
            {
                string sHex;
                //is there already data?
                if (m_BinStrTagData != null)
                    sHex = tools.BinStr2HexStr(m_BinStrTagData);
                else
                    return "001122334455667788991122";
                return sHex;
            }
            set 
            {
                if (value.Length == 24)
                {
                    InitBinTagData(value);
                }
                else
                    InitBinTagData();
            }
        }

        /// <summary>
        /// Test, if a Barcode is valid
        /// tests for DataID and if Supplier and PackageNumber parts can be converted to a valid number
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
            //test for DataID
            string sDataID = sBarcode.Substring(0, 4); 
            bool bDataIDOK = false;
            for (int i = 0; i < sDataIDs.Length; i++)
            {
                if (sDataID.Equals(sDataIDs[i]))
                    bDataIDOK = true;
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
            //test PackageNumber
            sNumber = sBarcode.Substring(13, 9);
            bool bPackageNumber = false;
            uNumber = 0;
            try {
                uNumber = UInt32.Parse(sNumber);
                if (uNumber>0x3fffffff)
                {
                    LastError="PackageNumber exceeds 0x3FFFFFFF";
                    return false;
                }
                string test = uNumber.ToString().PadLeft(9, '0');
                if (test.Equals(sNumber))
                    bPackageNumber = true;
                else
                    bPackageNumber = false;
            }
            catch {
                bPackageNumber = false;
            }
            if (bDataIDOK & bSupplierID & bPackageNumber)
                return true;
            else
                return false;
        }

    }
}
