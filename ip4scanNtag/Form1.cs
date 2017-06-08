using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Intermec.DataCollection;
using Intermec.DataCollection.RFID;
using Intermec.Device.Audio;

//R TAGID
//->H112233445566778899001122 HE2001040
//  ----- data = EPCID ------ - TAGID -
//W TAGID EPCID=H112233445566778899001122 WHERE TAGID=HE2001040
//->H112233445566778899001122 HE2001040 WROK

namespace ip4scanNtag
{
    public partial class mainForm : Form
    {
        VWGTLTAG vwTag = new VWGTLTAG ();
        BasicBRIReader m_Reader = null;
        BarcodeReader m_BCreader = null;
        CSymbology m_Symbologies;

        private bool bEnableLogging=false;
        Tone HighBeep=new Tone( 1000, 500, Intermec.Device.Audio.Tone.VOLUME.NORMAL );
        Tone HighBeep2=new Tone( 1200, 500, Intermec.Device.Audio.Tone.VOLUME.NORMAL );
        Tone LowBeep = new Tone(500, 500, Tone.VOLUME.VERY_LOUD);
        Tone LowBeepLow = new Tone(300, 500, Tone.VOLUME.NORMAL);
        bool m_bEventLogging=false;
        bool m_bFreeInput = false;
        bool m_bBoxIDEnabled;

        public string sCurrentCMD;
        enum eStatus
        {
            Offline,
            ReadTag,
            ReadBarcode,
            WriteTag,
            TooManyTags,
            BarCodeErr,
            WriteTagError,
            InvalidTag
        }
        eStatus CurrentStatus=eStatus.Offline;
        public string sHexCurrentEPCID = "112233445566778899001122"; //will contain the data to write in Hex
        public string sHexCurrentTagID; // will contain the TAGID in hex
        public UInt32 iCurrentBoxID = 0;
        
        public string sFilter = "1JUN";
        public string sSupplierID = "049977473";
        public string sItemNumber = "123456789";
        public string sBoxID = "100";

        public mainForm()
        {
            InitializeComponent();
            m_Symbologies = new CSymbology();
            m_Symbologies.DisableAll();
            m_Symbologies.Code128.Enable = true;
            if (DebugModus())
            {
                txtBoxID.Enabled = true;
                m_bBoxIDEnabled = true;
                mnuWriteTag.Enabled = true;
                mnuReadTags.Enabled = true;
                lblCMD.Visible = true;
                txtCMD.Visible = true;
                btnCMD.Visible = true;
            }
            else
            {
                txtBoxID.Enabled = false;
                m_bBoxIDEnabled = false;
                mnuWriteTag.Enabled = false;
                mnuReadTags.Enabled = false;
                lblCMD.Visible = false;
                txtCMD.Visible = false;
                btnCMD.Visible = false;
            }
            ChangeStatus(eStatus.Offline);
            txtStatus.Focus();
        }

        /// <summary>
        /// Will change menus and texts
        /// Do NOT invoke any methods from inside ChangeStatus()
        /// </summary>
        /// <param name="a"></param>
        private void ChangeStatus(eStatus a)
        {
            if (m_BCreader!=null)
                m_BCreader.ScannerEnable = false;
            CurrentStatus = a;
            txtBoxID.Enabled = false;
            switch (a)
            {
                case eStatus.Offline:
                    txtStatus.Text = "Offline, please connect";
                    txtStatus.BackColor = Color.Red;
                    mnuConnectReader.Enabled = true;
                    mnuDisconnectReader.Enabled = false;
                    mnuEnableLogging.Enabled = true;
                    mnuRestart.Enabled = false;
                    LowBeep.Play();
                    break;
                case eStatus.ReadBarcode:
                    txtStatus.Text = "Waiting for Barcode";
                    txtStatus.BackColor = Color.LightGreen;
                    break;
                case eStatus.ReadTag:
                    txtStatus.Text = "Waiting for single Tag";
                    txtStatus.BackColor = Color.LightCyan;
                    mnuRestart.Enabled = true;
                    HighBeep2.Play();
                    break;
                case eStatus.WriteTag :
                    txtStatus.Text = "Writing single Tag";
                    txtStatus.BackColor = Color.Green ;
                    mnuRestart.Enabled = true;
                    if (m_bBoxIDEnabled)
                        txtBoxID.Enabled = true;
                    else
                        txtBoxID.Enabled = false;
                    HighBeep2.Play();
                    break;
                case eStatus.WriteTagError:
                    txtStatus.Text = "Tag write error. Retrying";
                    txtStatus.BackColor = Color.Red;
                    LowBeepLow.Play();
                    mnuRestart.Enabled = true;
                    break;
                case eStatus.TooManyTags :
                    txtStatus.Text = "Please look for a single Tag";
                    txtStatus.BackColor = Color.Red;
                    LowBeepLow.Play();
                    mnuRestart.Enabled = true;
                    break;
                case eStatus.BarCodeErr :
                    txtStatus.Text = "Barcode does not meet Spec!";
                    txtStatus.BackColor = Color.Red;
                    //the following will be done inside CenterTrigger event
                    //if (m_BCreader!=null)
                    //    m_BCreader.ScannerEnable = true;
                    LowBeepLow.Play();
                    break;
                case eStatus.InvalidTag:
                    txtStatus.Text = "TagData does not meet Spec!";
                    txtStatus.BackColor = Color.Red;
                    LowBeepLow.Play();
                    break;
                default:
                    txtStatus.Text = "Unknown Status";
                    txtStatus.BackColor = Color.LightPink ;
                    mnuRestart.Enabled = true;
                    break;
/*            Offline,
            ReadTag,
            ReadBarcode,
            WriteTag,
            TooManyTags,
            BarCodeSizeErr,
            WriteTagError
*/
            }
            txtStatus.Refresh();
            Application.DoEvents();
        }

        private int OpenReader()
        {
            txtLog.Text = "...\r\n";
            BasicBRIReader.LoggerOptions LogOp = new BasicBRIReader.LoggerOptions();
            LogOp.LogFilePath = ".\\IDLClassDebugLog.txt";
            //LogOp.LogFilePath="\\Program Files\\IP4IDLAPP\\IDLClassDebugLog.txt";
            LogOp.ShowNonPrintableChars = true;
            int res = 0;
            try
            {
                if (m_Reader != null)
                {
                    CloseReader();
                }
                if (bEnableLogging)
                    m_Reader = new BasicBRIReader(this, LogOp );
                else
                    m_Reader = new BasicBRIReader(this);
                m_Reader.Open();
                AddEventHandlers();
                mnuEnableLogging.Enabled = false;
                mnuConnectReader.Enabled = false;
                mnuDisconnectReader.Enabled = true;
            }
            catch (BasicReaderException brx)
            {
                Add2List("OpenReader(): " + brx.Message);
                res = -1;
            }
            catch (SystemException sx)
            {
                Add2List("OpenReader(): " + sx.Message);
                res = -2;
            }
            Cursor.Current = Cursors.Default ;
            if (res != 0)
                System.Threading.Thread.Sleep(5000);
            Application.DoEvents();
            return res;
        }
        
        const uint EM_REPLACESEL = 0xc2;
        [DllImport("coredll")]
        extern static IntPtr GetCapture();
        [DllImport("coredll")]
        extern static int SendMessage(IntPtr hWnd, uint Msg, bool WParam, string LParam);
        private IntPtr GetHWND(Control ctl)
        { 
            ctl.Capture = true; IntPtr hWnd = GetCapture(); ctl.Capture = false; return hWnd; 
        }
        
        public void Add2List(string s)
        {
            if (txtLog.Text.Length > 16000)
                txtLog.Text = "...cleared log";
            //txtLog.Text += s + "\r\n";
            s = s + "\r\n";
            //txtLog.SelectedIndex = txtLog.Items.Count - 1;
            txtLog.Refresh();
            txtLog.SelectionStart = txtLog.Text.Length; 
            txtLog.SelectionLength = 0;
            SendMessage(GetHWND(txtLog), EM_REPLACESEL, false, s);
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            try
            {
                m_Symbologies.EnableAll();
                m_BCreader.ScannerEnable = true;
                m_BCreader.Dispose();
                m_Reader.Close();
                HighBeep.Dispose();
                LowBeep.Dispose();
                this.Dispose();
            }
            catch { 
            }
            Application.Exit();
        }


        private int RemoveEventHandlers()
        {
            try
            {
                this.m_Reader.EventHandlerRadio -= brdr_EventHandlerRadio;
                this.m_Reader.EventHandlerTag -= brdr_EventHandlerTag;
                this.m_Reader.EventHandlerCenterTrigger -= brdr_EventHandlerCenterTrigger;
                this.m_Reader.EventHandlerDCE -= brdr_EventHandlerDCE;
                this.m_Reader.EventHandlerOverflow -= brdr_EventHandlerOverflow;
            }
            catch
            {
                MessageBox.Show("Exception trying to create event handlers");
                return -1;
            }
            return 0;
        }
        /// <summary>
        /// Add the event handler to handle the tag events and trigger pulls.
        /// Not all of these are used but added as samples of what are available.
        /// </summary>
        /// <returns></returns>
        private int AddEventHandlers()
        {
            try
            {
                this.m_Reader.EventHandlerRadio += new Radio_EventHandler(brdr_EventHandlerRadio);
                this.m_Reader.EventHandlerTag += new Tag_EventHandler(brdr_EventHandlerTag);
                this.m_Reader.EventHandlerCenterTrigger += new CenterTrigger_EventHandler(brdr_EventHandlerCenterTrigger);
                this.m_Reader.EventHandlerDCE += new DCE_EventHandler(brdr_EventHandlerDCE);
                this.m_Reader.EventHandlerOverflow += new Overflow_EventHandler(brdr_EventHandlerOverflow);
            }
            catch
            {
                MessageBox.Show("Exception trying to create event handlers");
                return -1;
            }
            return 0;
        }

        void brdr_EventHandlerRadio(object sender, EVT_Radio_EventArgs EvtArgs)
        {
            int iTimeLeft = EvtArgs.RadioDutyCycleTimeleft;
            MessageBox.Show("brdr_EventHandlerRadio():\r\n" + EvtArgs.ToString());
            if (m_bEventLogging)
                Add2List("brdr_EventHandlerRadio() TimeLeft: " + iTimeLeft.ToString());
        }

        void brdr_EventHandlerOverflow(object sender, EVT_Overflow_EventArgs EvtArgs)
        {
            switch (EvtArgs.EventType){
                case EVTADV_Overflow_EventArgs.EVT_TRIGGER:
                    if (m_bEventLogging)
                        Add2List("EventHandlerOverflow():" + "EVT_TRIGGER");
                    break;
                case EVTADV_Overflow_EventArgs.EVT_RADIO:
                    if (m_bEventLogging)
                        Add2List("EventHandlerOverflow():" + "EVT_RADIO");
                    break;
                case EVTADV_Overflow_EventArgs.EVT_DCE:
                    if (m_bEventLogging)
                        Add2List("EventHandlerOverflow():" + "EVT_DCE");
                    break;
                case EVTADV_Overflow_EventArgs.EVT_CENTER_TRIGGER:
                    if (m_bEventLogging)
                        Add2List("EventHandlerOverflow():" + "EVT_CENTER_TRIGGER");
                    break;
                case EVTADV_Overflow_EventArgs.EVT_TAG:
                    if (m_bEventLogging)
                        Add2List("EventHandlerOverflow():" + "EVT_TAG");
                    break;
                case EVTADV_Overflow_EventArgs.EVT_UNKNOWN:
                    if (m_bEventLogging)
                        Add2List("EventHandlerOverflow():" + "EVT_UNKNOWN");
                    break;
                default:
                    if (m_bEventLogging)
                        Add2List("EventHandlerOverflow():" + " unknown EventType");
                    break;
            }

            MessageBox.Show("brdr_EventHandlerOverflow():\r\n" + EvtArgs.ToString());
            if (m_bEventLogging)
                Add2List("brdr_EventHandlerOverflow():" + EvtArgs.ToString());
        }

        void brdr_EventHandlerDCE(object sender, EVT_DCE_EventArgs EvtArgs)
        {
            switch ((int)EvtArgs.EventType)
            {
/*            
            UNKNOWN = 1,
            SHUTDOWN = 2,
            BUTTON = 3,
            DEVICE = 4,
*/
                case 1:
                    // The DCE has encountered an undefined condition...
                    if (m_bEventLogging) 
                        Add2List("brdr_EventHandlerDCE():" + EvtArgs.DataString);
                    CloseReader();
                    break;
                case 2:
                    // The DCE is shutting down...
                    if (m_bEventLogging)
                        Add2List("DCE evt: DCE ShutDown");
                    CloseReader();
                    break;
                case 3:
                    if (m_bEventLogging)
                        Add2List("brdr_EventHandlerDCE() BUTTON:" + EvtArgs.DataString);
                    break;
                case 4:
                    if (m_bEventLogging)
                        Add2List("brdr_EventHandlerDCE() DEVICE:" + EvtArgs.DataString);
                    break;
            }
        }

        /// <summary>
        /// This function process any tag that is returned as an event.
        /// This function is in use when you send a READ with REPORT=EVENT
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="EvtArgs"></param>
        void brdr_EventHandlerTag(object sender, EVT_Tag_EventArgs EvtArgs)
        {
            //bool bStatus = false;
            txtTagID.Text=EvtArgs.DataString.ToString();
        }

        /// <summary>
        /// This function fires when the center trigger on the IP4 is pulled or released
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="EvtArgs"></param>
        void brdr_EventHandlerCenterTrigger(object sender, EVT_CenterTrigger_EventArgs EvtArgs)
        {
            if (m_Reader.IsConnected == false)
            {
                //irda connection is still asleep after a 700 resume
                ChangeStatus(eStatus.Offline);
                return;
            }

            if (EvtArgs.CenterTriggerState.Equals(EVT_CenterTrigger_EventArgs.STATE.PULLED))
            {
                if ((CurrentStatus == eStatus.WriteTag) || (CurrentStatus == eStatus.WriteTagError))
                {
                    //R TAGID
                    //->H112233445566778899001122 HE2001040
                    //  ----- data = EPCID ------ - TAGID -
                    //W TAGID EPCID=H112233445566778899001122 WHERE TAGID=HE2001040
                    //->H112233445566778899001122 HE2001040 WROK
 
                    //sCurrentCMD = "W TAGID HEX(1:4,12)=H" + sHexCurrentEPCID;//
                    try
                    {
                        //change data
                        vwTag.sFilter = sFilter;
                        vwTag.iSupplierID = UInt32.Parse(sSupplierID);
                        vwTag.iItemNumber = UInt32.Parse(sItemNumber);
                        if (m_bBoxIDEnabled)
                            vwTag.BoxID = UInt32.Parse(txtBoxID.Text);
                        sHexCurrentEPCID = vwTag.GetHex();
                    }
                    catch (SystemException sx)
                    {
                        Add2List("Error in building TagData" + sx.Message);
                        ChangeStatus(eStatus.InvalidTag);
                        return;
                    }
                    //"34AC2FA98811D6F345400001"
                    //Binary: 001101001010110000101111101010011000100000010001110101101111001101000101010000000000000000000001
                    //string: 100001000001000100010100000000010001010111011001100100001101101000011010101000101100100011101100
                    //5J UN 04-997-7473 123456789
                    sCurrentCMD = "W TAGID EPCID=H" + sHexCurrentEPCID + " WHERE TAGID=H" + sHexCurrentTagID; ;
                    if (ExecuteCMD(sCurrentCMD))
                    {
                        Add2List("OK");
                    }
                    else
                    {
                        Add2List("failed");
                    }
                }
                else if (CurrentStatus == eStatus.ReadTag | CurrentStatus==eStatus.InvalidTag )
                {
                    sCurrentCMD = "READ TAGID";
                    if (ExecuteCMD(sCurrentCMD))
                    {
                        Add2List("OK");
                    }
                    else
                    {
                        Add2List("failed");
                    }
                }

                else if (CurrentStatus == eStatus.TooManyTags )
                {
                    sCurrentCMD = "READ TAGID";
                    if (ExecuteCMD(sCurrentCMD))
                    {
                        Add2List("OK");
                    }
                    else
                    {
                        Add2List("failed");
                    }
                }
                else if (CurrentStatus == eStatus.ReadBarcode | CurrentStatus==eStatus.BarCodeErr )
                    m_BCreader.ScannerOn = true;

            }
            else if (EvtArgs.CenterTriggerState.Equals(EVT_CenterTrigger_EventArgs.STATE.RELEASED))
            {
                if (CurrentStatus == eStatus.ReadBarcode)
                    m_BCreader.ScannerOn = false;
                //StopRead();
            }
        }
        
        private int SetAttributes()
        {
            bool bStatus = false;
            if (m_Reader.IsConnected == false) { return -1; }
            sCurrentCMD = "ATTRIB TAGTYPE=EPCC1G2"; // comboBox1.Text;
            bStatus = ExecuteCMD(sCurrentCMD);
            if (bStatus == false)
            {
                Add2List("SetAttrib failed, reader connected?");
                return -1;
            }
            else
            {
                //This will set the Option IntermecSettings-RFID-Reader 1-Enable Reader
                //Configuring the DCE Transport Programmatically Using BRI
                Intermec.DataCollection.RFID.BasicBRIReader DCEConfig = new BasicBRIReader(null);
                try
                {
                    DCEConfig.Open("TCP://127.0.0.1:2188");
                    // BRI ‘device’ command.
                    DCEConfig.Execute("device 1 attrib adminstatus=on");      
                    DCEConfig.Close();
                    return 0;
                }
                catch (BasicReaderException bex)
                {
                    Add2List("SetAttrib() DCEConfig could not connect to DCE: " + bex.Message);
                    return -2;
                }
                catch (Exception ex)
                {
                    Add2List("SetAttrib() DCEConfig exception " + ex.Message);
                    return -3;
                }
            }
        }

        private bool ExecuteCMD(string tCMD)
        {
            string tMsg = null;
            Add2List("Sending->" + tCMD);
            try
            {
                if (m_Reader.IsConnected)
                    tMsg = m_Reader.Execute(tCMD); 
                else
                {
                    ChangeStatus(eStatus.Offline);
                    Add2List("Reader disconnected, please Connect");
                    return false;
                }
            }
            catch (BasicReaderException eBRI)
            {
                Add2List("BasicReaderException: " + eBRI.ToString());
                //MessageBox.Show("BasicReaderException: IDL ERR occured for CMD: " + tCMD + "\r\n" + eBRI.ToString());
                ChangeStatus(eStatus.Offline);
                return false;
            }
            //"H112233445566778899001122 HE2001040\r\nOK>"
            Add2List("--->" + tMsg);
            //Show TAG ID
            if (sCurrentCMD.Equals("READ TAGID"))
            {
                sbyte[] aFieldSeparator ={ (sbyte)' ' };
                sbyte[] aRespBuf = tools.StrToSByteArray(tMsg);
                Tag[] tags = BRIParser.GetTags(aRespBuf, aFieldSeparator);
                if (tags == null)
                    return false;
                if (tags.GetLength(0) == 1)
                {
                    //only one tag should be visible
                    //save the last 22Bits as Box ID
                    if (VWGTLTAG.IsValidTag(tags[0].ToString()) == false)
                    {
                        DialogResult ant = MessageBox.Show("Tag does not meet specs. Overwrite?", "VW tag demo", MessageBoxButtons.YesNo,MessageBoxIcon.Question , MessageBoxDefaultButton.Button1 );
                        if (ant == DialogResult.Yes)
                        {
                            UInt32 uBox = vwTag.BoxID;
                            vwTag = new VWGTLTAG();
                            vwTag.BoxID = uBox;
                        }
                        else
                        {
                            ChangeStatus(eStatus.InvalidTag);
                            return false;
                        }
                    }
                    else
                        vwTag = new VWGTLTAG(tags[0].ToString()); //create a new tag with the tag data read
                    //read the box id from the tag
                    iCurrentBoxID = vwTag.BoxID; 
                    txtBoxID.Text = iCurrentBoxID.ToString();

                    //fill txtbox with data from tag
                    txtTagDataStr.Text = vwTag.sFilter + " " +
                                    vwTag.iSupplierID.ToString() + " " +
                                    vwTag.iItemNumber.ToString();

                    System.Diagnostics.Debug.WriteLine("READ TAGID tag data: " + vwTag.ToString());
                    //tagKey is the same as tag data
                    //System.Diagnostics.Debug.WriteLine("READ TAGID: tags[0].tagkey: " + tools.Hex2Asc(sByteArrayToString(tags[0].TagKey)));

                    //store TAGID
                    sHexCurrentTagID = tags[0].TagFields.FieldArray[0].DataString.Substring(1) ;
                    ChangeStatus(eStatus.WriteTag);
                }
                else
                {
                    //only one tag should be visible
                    ChangeStatus(eStatus.TooManyTags);
                }

                //read tag data (aka EPCID)
                if (tMsg.StartsWith("H"))
                {
                    string s;
                    s = tMsg.Substring(1, tMsg.IndexOf(" ")-1);
                    txtTagData.Text = s;
                    s = tMsg.Substring(s.Length+1);
                    s = tools.CopyToR(s);
                    s = s.Substring(s.IndexOf("H")+ 1);
                    txtTagID.Text = s;
                }
            }

            if (sCurrentCMD.Equals("ATTRIB TAGTYPE=EPCC1G2") == true)
            {
                if (tMsg.IndexOf("OK") >= 0)
                    return true;
                else
                    return false;
            }

            if (tMsg.IndexOf("ERR") >= 0 && sCurrentCMD.Equals("ATTRIB SCHEDOPT=1") == false)
            {
                //MessageBox.Show("Warning, BRI ERR occured for: " + tCMD + "\r\n" + tMsg);
                Add2List("Warning, BRI ERR occured for: " + tCMD + "\r\n" + tMsg);
                return false;
            }

            if (sCurrentCMD.StartsWith("W "))
            {
                if (tMsg.IndexOf("ERR") >= 0 && CurrentStatus == eStatus.WriteTag)
                {
                    ChangeStatus(eStatus.WriteTagError);
                    return false;
                }

                if (tMsg.IndexOf("WROK") >= 0 && (CurrentStatus == eStatus.WriteTagError) || (CurrentStatus == eStatus.WriteTag))
                {
                    HighBeep.Play();
                    ChangeStatus(eStatus.ReadBarcode);
                    if (tMsg.StartsWith("H"))//"H303033353032393035343135 HE2001040 WROK\r\nOK>"
                    {
                        string s;
                        s = tMsg.Substring(1, tMsg.IndexOf(" ") - 1);
                        txtTagData.Text = s;
                        System.Diagnostics.Debug.WriteLine("WROK: tag data string: " + s);
                        s = tMsg.Substring(s.Length + 1);
                        s = tools.CopyToR(s);
                        s = s.Substring(s.IndexOf("H") + 1);
                        //"E2001040 WROK"
                        txtTagID.Text = s.Substring(1, s.IndexOf(" ") - 1);
                        System.Diagnostics.Debug.WriteLine(s);
                    }
                }
            }
            return true;
        }
        

        private void RestartRead()
        {
            //m_Reader.StopReadingTags();
            m_BCreader.ScannerOn = false;
            ChangeStatus(eStatus.ReadBarcode);
        }

        private int ParseResponseMessage(string tMsg)
        {
            int x = 0;
            string tString = "";
            char tChar = '0';

            //just to make parsing code uniform
            tMsg += "\r\n";

            char[] tMyCharList = tMsg.ToCharArray();

            //clear response list
            int RspCount = 0;

            string[] RspMsgList = new string[1000];
            RspMsgList.Initialize();

            //parse the response message
            for (x = 0; x < tMyCharList.Length; x++)
            {
                tChar = tMyCharList[x];
                if (tChar.Equals('\n') == false && tChar.Equals('\r') == false)
                {
                    tString += tChar;
                }
                else if (tChar.Equals('\r') == true)
                {
                    RspCount++;
                    RspMsgList[RspCount] = tString;
                    tString = "";
                }
            }

            //process the response messages
            for (x = 1; x <= RspCount; x++)
            {
                if (RspMsgList[x].IndexOf("H") == 0 && RspMsgList[x].IndexOf("HOP") < 0)
                {
                    //Tag Data
                    if (sCurrentCMD.Equals("R"))
                    {
                        //skip this data, its part of a bug work around regarding cont read modes
                    }
                    else
                    {
                        Add2List(RspMsgList[x]);
                    }
                }
                else if (RspMsgList[x].IndexOf("OK>") == 0)
                {
                    //end of reader response
                    if (sCurrentCMD.StartsWith("R") == false && sCurrentCMD.StartsWith("W") == false)
                    {
                        Add2List(RspMsgList[x]);
                    }
                    break;
                }
                else if (sCurrentCMD.IndexOf("ATTRIB") == 0)
                {
                    Add2List(RspMsgList[x]);
                }
                else if (sCurrentCMD.IndexOf("UTIL") == 0)
                {
                    Add2List(RspMsgList[x]);
                    txtLog.Refresh();
                }
            }//END for (x = 1; x <= RspCount; x++)

            return 0;
        }

        private void mnuRestart_Click(object sender, EventArgs e)
        {
            RestartRead();
        }

        private int OpenBCreader()
        {
            if (m_BCreader != null)
                return 0;
            try
            {
                m_BCreader = new BarcodeReader();
                m_BCreader.BarcodeRead += new BarcodeReadEventHandler(m_BCreader_BarcodeRead);
                m_BCreader.ThreadedRead(true);
                m_BCreader.ScannerEnable = false;
                m_BCreader.symbology.Code128.Ean128Identifier = false;
            }
            catch (BarcodeReaderException bx)
            {
                Add2List("OpenBCReader(): " + bx.Message);
                return -1;
            }
            return 0;
        }

        void m_BCreader_BarcodeRead(object sender, BarcodeReadEventArgs bre)
        {
            string sBarcode;
            int iSymID = bre.Symbology;
            sBarcode  = bre.strDataBuffer;
            //remove unneeded chars
            sBarcode = sBarcode.Replace(" ", "");
            sBarcode = sBarcode.Replace("-", "");
            //test the barcode
            if (!VWGTLTAG.CheckBarCode(sBarcode))
            {
                Add2List(VWGTLTAG.LastError);
                ChangeStatus(eStatus.BarCodeErr);
                return;
            }
            //store data
            sFilter = sBarcode.Substring(0, 4);
            sSupplierID = sBarcode.Substring(4, 9);
            sItemNumber = sBarcode.Substring(13, 9);
            System.Diagnostics.Debug.WriteLine("Barcode Data:\nFilter: " + sFilter + "\nSupplierID: " + sSupplierID + "\nItemNumber: " + sItemNumber);

            //set the barcode data
            ChangeStatus(eStatus.ReadTag);
            txtBarcodeData.Text = sBarcode;
            txtTagID.Text = "";
            txtTagData.Text = "";
            txtTagDataStr.Text = "";

        }

        private void mnuConnectReader_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            if (m_BCreader == null)
                OpenBCreader();
            if (OpenReader() == 0)
            {
                //try first command
                int res = SetAttributes();
                if (res != 0)
                {
                    m_Reader.Close();
                    m_Reader = null;
                }
            }

            if (m_Reader != null)
            {
                if (m_Reader.IsConnected)
                    ChangeStatus(eStatus.ReadBarcode);//  ReadTag);
                else
                    ChangeStatus(eStatus.Offline);
            }
            else
                ChangeStatus(eStatus.Offline);

            Cursor.Current = Cursors.Default;
        }

        private void CloseReader()
        {
            if (m_Reader != null)
                try
                {
                    if (m_Reader != null)
                        m_Reader.Close();
                    RemoveEventHandlers();
                }
                catch (BasicReaderException brx)
                {
                    Add2List("CloseReader(): " + brx.Message);
                }
                catch (SystemException sx)
                {
                    Add2List("CloseReader(): " + sx.Message);
                }
                finally { 
                    ChangeStatus(eStatus.Offline);
                    txtTagID.BackColor = Color.LightGray;
                }
        }

        private void mnuDisconnectReader_Click(object sender, EventArgs e)
        {
            CloseReader();
        }

        private void mnuEnableLogging_Click(object sender, EventArgs e)
        {
            mnuEnableLogging.Checked = !mnuEnableLogging.Checked;
            if (mnuEnableLogging.Checked)
                bEnableLogging = true;
            else
                bEnableLogging = false;
            mnuReadTags.Enabled = bEnableLogging ;
            mnuWriteTag.Enabled = bEnableLogging;
        }

        private void mnuWriteTag_Click(object sender, EventArgs e)
        {
            //change data
            vwTag.sFilter = sFilter;
            vwTag.iSupplierID = UInt32.Parse(sSupplierID);
            vwTag.iItemNumber = UInt32.Parse(sItemNumber);
            if (m_bBoxIDEnabled)
                vwTag.BoxID = UInt32.Parse(txtBoxID.Text);
            sHexCurrentEPCID = vwTag.GetHex();
            sCurrentCMD = "W TAGID EPCID=H" + sHexCurrentEPCID + " WHERE TAGID=H" + sHexCurrentTagID; ;
            if ((m_Reader != null) && (m_Reader.IsConnected))
            {
                ChangeStatus(eStatus.WriteTag);
                ExecuteCMD(sCurrentCMD);
            }
            else
                ChangeStatus(eStatus.Offline);
        }

        public bool m_bDebugModus = false;
        public bool DebugModus()
        {
            if (System.Diagnostics.Debugger.IsAttached)
                m_bDebugModus = true;
            else
                m_bDebugModus = false;
            return m_bDebugModus;
        }

        private void mnuReadTags_Click(object sender, EventArgs e)
        {
            if ((m_Reader != null) && (m_Reader.IsConnected))
            {
                sCurrentCMD = "READ TAGID";
                ChangeStatus(eStatus.ReadTag);
                ExecuteCMD(sCurrentCMD);
            }
        }

        private void btnCMD_Click(object sender, EventArgs e)
        {
            if (m_Reader != null)
            {
                if (m_Reader.IsConnected)
                {
                    sCurrentCMD = txtCMD.Text;
                    ExecuteCMD(sCurrentCMD);
                }
                else
                    Add2List("Reader not connected!");
            }
            else
                Add2List("Reader not available. Please Connect");
        }

        private void txtTagID_TextChanged(object sender, EventArgs e)
        {

        }

        private void mnuEventLogging_Click(object sender, EventArgs e)
        {
            mnuEnableLogging.Checked = !mnuEnableLogging.Checked;
            m_bEventLogging = mnuEnableLogging.Checked;
        }

        private void menuFreeCMDs_Click(object sender, EventArgs e)
        {
            m_bFreeInput = !m_bFreeInput;
            mnuFreeCMDs.Checked = m_bFreeInput;
            if (m_bFreeInput)
            {
                lblCMD.Visible = true;
                txtCMD.Visible = true;
                btnCMD.Visible = true;
            }
            else {
                lblCMD.Visible = false;
                txtCMD.Visible = false;
                btnCMD.Visible = false;
            }
        }

        private void mnuEnableBOXID_Click(object sender, EventArgs e)
        {
            mnuEnableBOXID.Checked = !mnuEnableBOXID.Checked;
            m_bBoxIDEnabled = mnuEnableBOXID.Checked;
            txtBoxID.Enabled = m_bBoxIDEnabled;
        }
    }
}