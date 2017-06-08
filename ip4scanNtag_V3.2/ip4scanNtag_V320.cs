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
using Microsoft.Win32;

//R TAGID
//->H112233445566778899001122 HE2001040
//  ----- data = EPCID ------ - TAGID -
//W TAGID EPCID=H112233445566778899001122 WHERE TAGID=HE2001040
//->H112233445566778899001122 HE2001040 WROK

namespace ip4scanNtag
{
    public partial class mainForm : Form
    {
        String txtConnectMsg = "RFID Reader verbinden";
        String txtDisconnectMsg = "RFID Reader trennen";

        VWGTLTAG2 vwTag;// = new VWGTLTAG2 ();
        VWGTLTAG2 vwTag2;// = new VWGTLTAG2 ();
        string sOldTagHex = "001122334455667788991122";
        string sOldTagHex2 = "001122334455667788991122";
        BasicBRIReader m_Reader = null;
        BarcodeReader m_BCreader = null;

        //CSymbology m_Symbologies;
        
        private const int SymbologyLicensePlate =
            Intermec.DataCollection.BarcodeReader.BARCODE_SYMBOLOGY_CODE128;
        
        private const int SymbologyBoxID =
            Intermec.DataCollection.BarcodeReader.BARCODE_SYMBOLOGY_CODE39;

        Tone HighBeep = new Tone(1000, 500, Intermec.Device.Audio.Tone.VOLUME.NORMAL);
        Tone HighBeep2=new Tone( 1200, 500, Intermec.Device.Audio.Tone.VOLUME.NORMAL );
        Tone LowBeep = new Tone(500, 500, Tone.VOLUME.VERY_LOUD);
        Tone LowBeep2 = new Tone(300, 500, Tone.VOLUME.NORMAL);
        private bool bEnableLogging=false;
        bool m_bEventLogging=false;
        bool m_bFreeInput = false;
        bool m_bEditingEnabled;
        bool m_bUseBitMask = false;
        bool m_bBoxIDscanningEnabled = false;
        bool m_bEnableTwoTAGscanning = false;
        bool m_bTAG1writeOK = false;
        bool m_bTAG2writeOK = false;
        Tag[] tags;

        bool m_bCenterEventProcessing=false;

        UInt32 iCurrentBoxID = 0;

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
            InvalidTag,
            InvalidData,
            TagReadError,
            ReadBarcodeBoxID,
            BarcodeBoxIDError,
            //new with v3
            ReadTwoTags,
            ReadTwoTagsError,
            WriteTwoTags,
            WriteTwoTagsError
        }
        eStatus CurrentStatus=eStatus.Offline;
        public string sHexCurrentEPCID = "112233445566778899001122"; //will contain the data to write in Hex
        public string sHexCurrentEPCID2 = "112233445566778899001122"; //will contain the data to write in Hex
        public string sHexCurrentTagID; // will contain the TAGID in hex
        /// <summary>
        /// internal storage of the Box Identification
        /// </summary>
        //public UInt32 iCurrentBoxID = 0;
        /// <summary>
        /// internal storage of the Filter
        /// used to identify several TAGs on one box
        /// </summary>
        //public UInt32 iCurrentFilter = 0;
        /// <summary>
        /// Data identifier consists of "1J", "5J" or "6J"
        /// and the Issuing Agency Code for 'Leopard',
        /// always = "UN"
        /// </summary>
        public string sDataID = "1JUN";
        /// <summary>
        /// Supplier Indentification according to DUNS
        /// </summary>
        public string sSupplierID = "049977473";
        /// <summary>
        /// package number
        /// </summary>
        public string sPackageNumber = "123456789";
        /// <summary>
        /// box identification
        /// </summary>
        public string sBoxID = "100";

        public mainForm()
        {
            InitializeComponent();
            ReadIni();
            //m_Symbologies = new CSymbology();
            EnableEditing(m_bEditingEnabled);
            if (DebugModus())
            {
                txtBarcodeData.Text = "5J UN 04-997-7473 123456789";
                EnableEditing(true);
            }
           
            lblCMD.Visible = m_bFreeInput;
            txtCMD.Visible = m_bFreeInput;
            btnCMD.Visible = m_bFreeInput;

            if (System.Diagnostics.Debugger.IsAttached)
                menuItem2.Enabled = true;
            else
                menuItem2.Enabled = false;

            ChangeStatus(eStatus.Offline);
            txtStatus.Focus();
        }

        public void EnableEditing(bool bEnable)
        {
            m_bEditingEnabled = bEnable;

            //mnuWriteTag.Enabled = bEnable;
            //mnuReadTags.Enabled = bEnable;
            
            txtBoxID.Enabled = bEnable;
            txtBoxID.Visible = bEnable;

            lblBoxID.Enabled = bEnable;
            lblBoxID.Visible = bEnable;

            txtFilter.Enabled = bEnable;
            txtFilter.Visible = bEnable;

            lblFilter.Enabled = bEnable;
            lblFilter.Visible = bEnable;

            mnuEditingEnabled.Checked = bEnable;
        }
        public void EnableTwoTAGsMode(bool bEnable)
        {
            //BoxID editing etc. not allowed in TwoTAGsMode
            if (bEnable)
            {
                lblTagID.Text = "TAG1:";
                lblTagData.Text = "TAG2:";
                m_bEditingEnabled = false;
                EnableEditing(m_bEditingEnabled);

                mnu_BoxIDscanning.Enabled = false;
                mnu_BoxIDscanning.Checked = false;
                m_bBoxIDscanningEnabled = false;
                
                mnuEditingEnabled.Enabled = false;
                mnuEditingEnabled.Checked = false;
                m_bEditingEnabled = false;

                //mnuUseBitMask.Enabled = false;
                //mnuUseBitMask.Checked = false;
                //m_bUseBitMask = false;
            }
            else
            {
                //ReadIni();
                EnableEditing(m_bEditingEnabled);

                lblTagID.Text = "TAG ID:";
                lblTagData.Text = "TAG data:";
                mnu_BoxIDscanning.Enabled = true;
                mnu_BoxIDscanning.Checked = m_bBoxIDscanningEnabled;
                                
                mnuEditingEnabled.Enabled = true;
                mnuEditingEnabled.Checked = m_bEditingEnabled;

                //mnuUseBitMask.Enabled = true;
                //mnuUseBitMask.Checked = m_bUseBitMask;
            }
        }
        /// <summary>
        /// Will change menus and texts
        /// Do NOT invoke any methods from inside ChangeStatus()
        /// </summary>
        /// <param name="a"></param>
        private void ChangeStatus(eStatus a)
        {
            System.Diagnostics.Debug.WriteLine("ChangeStatus: " + a);
            if (m_BCreader!=null)
                m_BCreader.ScannerEnable = false;
            CurrentStatus = a;
            //btnConnect.Text = "Disconnect Reader";
            btnConnect.Text = txtDisconnectMsg;
            switch (a)
            {
                case eStatus.Offline:
                    //txtStatus.Text = "Offline, please connect";
                    txtStatus.Text = "Offline, bitte verbinden";
                    txtStatus.BackColor = Color.Red;
                    mnuConnectReader.Enabled = true;
                    mnuDisconnectReader.Enabled = false;
                    btnConnect.Text = txtConnectMsg;// "Connect Reader";
                    mnuEnableBRILogging.Enabled = true;
                    mnuRestart.Enabled = false;
                    LowBeep.Play();
                    break;
                case eStatus.ReadBarcode:
                    m_BCreader.symbology.Code128.Enable = true;
                    m_BCreader.symbology.Code39.Enable = false;

                    //txtStatus.Text = "Waiting for Barcode";
                    txtStatus.Text = "Warte auf Barcode";
                    txtStatus.BackColor = Color.LightGreen;
                    //Clear fields
                    txtBarcodeData.Text = "";
                    txtTagID.Text = "";
                    txtTagData.Text = "";
                    txtTagDataStr.Text = "";
                    txtBoxID.Text = "";
                    txtFilter.Text = "";
                    break;
                case eStatus.ReadTag:
                    //txtStatus.Text = "Waiting for single TAG";
                    txtStatus.Text = "Warte auf ein TAG";
                    txtStatus.BackColor = Color.LightCyan;
                    mnuRestart.Enabled = true;
                    HighBeep2.Play();
                    break;
                case eStatus.ReadTwoTags :
                    //txtStatus.Text = "Waiting for two TAGs";
                    txtStatus.Text = "Warte auf zwei TAGs";
                    txtStatus.BackColor = Color.LightCyan;
                    mnuRestart.Enabled = true;
                    HighBeep2.Play();
                    break;
                case eStatus.TagReadError:
                    //txtStatus.Text = "TAG read error. Try again.";
                    txtStatus.Text = "TAG-Lesefehler. Wiederholen!";
                    txtStatus.BackColor = Color.Red;
                    LowBeep2.Play();
                    mnuRestart.Enabled = true;
                    break;
                case eStatus.ReadTwoTagsError:
                    //txtStatus.Text = "Look for two TAGs. Try again.";
                    txtStatus.Text = "Zwei TAGs notwendig. Wiederholen!";
                    txtStatus.BackColor = Color.Red;
                    LowBeep2.Play();
                    mnuRestart.Enabled = true;
                    break;
                case eStatus.ReadBarcodeBoxID:
                    m_BCreader.symbology.Code128.Enable = false;
                    m_BCreader.symbology.Code39.Enable = true;

                    //txtStatus.Text = "Waiting for BoxID";
                    txtStatus.Text = "Warte auf BoxID";
                    txtStatus.BackColor = Color.LightGreen;
                    mnuRestart.Enabled = true;
                    break;
                case eStatus.BarcodeBoxIDError:
                    //txtStatus.Text = "Error scanning BoxID";
                    txtStatus.Text = "Fehler in der BoxID";
                    txtStatus.BackColor = Color.Red;
                    LowBeep2.Play();
                    break;
                case eStatus.WriteTag:
                    //txtStatus.Text = "Writing single TAG";
                    txtStatus.Text = "Ein TAG beschreiben";
                    txtStatus.BackColor = Color.Green ;
                    mnuRestart.Enabled = true;
                    EnableEditing(m_bEditingEnabled);
                    HighBeep2.Play();
                    break;
                case eStatus.WriteTwoTags:
                    m_bTAG1writeOK = false;
                    m_bTAG2writeOK = false;
                    //txtStatus.Text = "Writing two TAGs";
                    txtStatus.Text = "Zwei TAGs beschreiben";
                    txtStatus.BackColor = Color.Green ;
                    mnuRestart.Enabled = true;
                    EnableEditing(m_bEditingEnabled);
                    HighBeep2.Play();
                    break;
                case eStatus.WriteTagError:
                    //txtStatus.Text = "Tag write error. Same TAG?";
                    txtStatus.Text = "TAG-Schreibfehler. Anderes TAG?";
                    txtStatus.BackColor = Color.Red;
                    LowBeep2.Play();
                    mnuRestart.Enabled = true;
                    break;
                case eStatus.WriteTwoTagsError:
                    //txtStatus.Text = "Tag write error. Same TAGs?";
                    txtStatus.Text = "TAGs-Schreibfehler. Andere TAGs?";
                    txtStatus.BackColor = Color.Red;
                    LowBeep2.Play();
                    mnuRestart.Enabled = true;
                    break;
                case eStatus.TooManyTags :
                    //txtStatus.Text = "Please look for a single TAG";
                    txtStatus.Text = "Bitte genau ein TAG suchen!";
                    txtStatus.BackColor = Color.Red;
                    LowBeep2.Play();
                    mnuRestart.Enabled = true;
                    break;
                case eStatus.BarCodeErr :
                    //txtStatus.Text = "Barcode does not meet Spec!";
                    txtStatus.Text = "Barcode passt nicht ins Schema!";
                    txtStatus.BackColor = Color.Red;
                    //the following will be done inside CenterTrigger event
                    //if (m_BCreader!=null)
                    //    m_BCreader.ScannerEnable = true;
                    LowBeep2.Play();
                    break;
                case eStatus.InvalidTag:
                    //txtStatus.Text = "TAG-Data does not meet Spec!";
                    txtStatus.Text = "TAG-Data passt nicht ins Schema!";
                    txtStatus.BackColor = Color.Red;
                    LowBeep2.Play();
                    break;
                default:
                    //txtStatus.Text = "Unknown Status";
                    txtStatus.Text = "Unbekannter Status";
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
        
        private bool CheckInput()
        {
            UInt32 u32;
            try
            {
                u32 = UInt32.Parse(txtFilter.Text);
                if (u32 > 7)
                {
                    txtFilter.BackColor = Color.Red;
                    txtFilter.Focus();
                    return false;
                }
                else
                    txtFilter.BackColor = Color.White;

                u32 = UInt32.Parse(txtBoxID.Text);
                if (u32 > 0xfffff)
                {
                    txtBoxID.BackColor = Color.Red;
                    txtBoxID.Focus();
                    return false;
                }
                else
                    txtBoxID.BackColor = Color.White;
            }
            catch
            {
                txtBoxID.BackColor = Color.Red;
                txtFilter.BackColor = Color.Red;
                return false;
            }
            return true;
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
                mnuEnableBRILogging.Enabled = false;
                mnuConnectReader.Enabled = false;
                mnuDisconnectReader.Enabled = true;
                btnConnect.Text = txtDisconnectMsg;// "Disconnect Reader";
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
        
        /// <summary>
        /// Add Text at end of edit box and scroll
        /// </summary>
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
            System.Diagnostics.Debug.WriteLine("Add2List: " + s);
            s = s + "\r\n";
            //txtLog.SelectedIndex = txtLog.Items.Count - 1;
            txtLog.Refresh();
            txtLog.SelectionStart = txtLog.Text.Length; 
            txtLog.SelectionLength = 0;
            SendMessage(GetHWND(txtLog), EM_REPLACESEL, false, s);
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "EXIT application", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                return; 
            try
            {
                WriteIni();
                m_BCreader.symbology.EnableAll();
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
                //this.m_Reader.EventHandlerRadio -= brdr_EventHandlerRadio;
                this.m_Reader.EventHandlerTag -= brdr_EventHandlerTag;
                //deprecated from v2.1: this.m_Reader.EventHandlerCenterTrigger -= brdr_EventHandlerCenterTrigger;
                //deprecated from v2.1: this.m_Reader.EventHandlerDCE -= brdr_EventHandlerDCE;
                this.m_Reader.ReaderEvent -= brdr_ReaderEventHandler;
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
                //this.m_Reader.EventHandlerRadio += new Radio_EventHandler(brdr_EventHandlerRadio);
                this.m_Reader.EventHandlerTag += new Tag_EventHandler(brdr_EventHandlerTag);
                //deprecated from v2.1: this.m_Reader.EventHandlerCenterTrigger += new CenterTrigger_EventHandler(brdr_EventHandlerCenterTrigger);
                //deprecated from v2.1: this.m_Reader.EventHandlerDCE += new DCE_EventHandler(brdr_EventHandlerDCE);
                this.m_Reader.ReaderEvent += new BasicEvent (brdr_ReaderEventHandler);
                this.m_Reader.EventHandlerOverflow += new Overflow_EventHandler(brdr_EventHandlerOverflow);
            }
            catch
            {
                MessageBox.Show("Exception trying to create event handlers");
                return -1;
            }
            return 0;
        }

        void brdr_EventHandlerRadio(object sender, BasicReaderEventArgs EvtArgs)
        {
            int iTimeLeft = Convert.ToInt32( EvtArgs.EventData);
            //pause a little bit to avoid duty cycle failures
            Add2List("duty time left=" + iTimeLeft.ToString() + "Sleeping 500ms...");
            System.Threading.Thread.Sleep(500);
            Add2List("...OK");
            //MessageBox.Show("brdr_EventHandlerRadio():\r\n" + iTimeLeft.ToString());
            if (m_bEventLogging)
                Add2List("brdr_EventHandlerRadio() TimeLeft: " + iTimeLeft.ToString());
        }
        #region deprecated
        void brdr_EventHandlerOverflow(object sender, EVT_Overflow_EventArgs EvtArgs)
        {
            //switch (EvtArgs.EventType){
            //    case EVTADV_Overflow_EventArgs.EVT_TRIGGER:
            //        if (m_bEventLogging)
            //            Add2List("EventHandlerOverflow():" + "EVT_TRIGGER");
            //        break;
            //    case EVTADV_Overflow_EventArgs.EVT_RADIO:
            //        if (m_bEventLogging)
            //            Add2List("EventHandlerOverflow():" + "EVT_RADIO");
            //        break;
            //    case EVTADV_Overflow_EventArgs.EVT_DCE:
            //        if (m_bEventLogging)
            //            Add2List("EventHandlerOverflow():" + "EVT_DCE");
            //        break;
            //    case EVTADV_Overflow_EventArgs.EVT_CENTER_TRIGGER:
            //        if (m_bEventLogging)
            //            Add2List("EventHandlerOverflow():" + "EVT_CENTER_TRIGGER");
            //        break;
            //    case EVTADV_Overflow_EventArgs.EVT_TAG:
            //        if (m_bEventLogging)
            //            Add2List("EventHandlerOverflow():" + "EVT_TAG");
            //        break;
            //    case EVTADV_Overflow_EventArgs.EVT_UNKNOWN:
            //        if (m_bEventLogging)
            //            Add2List("EventHandlerOverflow():" + "EVT_UNKNOWN");
            //        break;
            //    default:
            //        if (m_bEventLogging)
            //            Add2List("EventHandlerOverflow():" + " unknown EventType");
            //        break;
            //}

            //MessageBox.Show("brdr_EventHandlerOverflow():\r\n" + EvtArgs.ToString());
            if (m_bEventLogging)
                Add2List("brdr_EventHandlerOverflow():" + EvtArgs.ToString());
        }

        //void brdr_EventHandlerDCE(object sender, EVT_DCE_EventArgs EvtArgs)
        //{
//            switch ((int)EvtArgs.EventType)
//            {
///*            
//            UNKNOWN = 1,
//            SHUTDOWN = 2,
//            BUTTON = 3,
//            DEVICE = 4,
//*/
//                case 1:
//                    // The DCE has encountered an undefined condition...
//                    if (m_bEventLogging) 
//                        Add2List("brdr_EventHandlerDCE():" + EvtArgs.DataString);
//                    CloseReader();
//                    break;
//                case 2:
//                    // The DCE is shutting down...
//                    if (m_bEventLogging)
//                        Add2List("DCE evt: DCE ShutDown");
//                    CloseReader();
//                    break;
//                case 3:
//                    if (m_bEventLogging)
//                        Add2List("brdr_EventHandlerDCE() BUTTON:" + EvtArgs.DataString);
//                    break;
//                case 4:
//                    if (m_bEventLogging)
//                        Add2List("brdr_EventHandlerDCE() DEVICE:" + EvtArgs.DataString);
//                    if (EvtArgs.DataString.IndexOf("DISCONNECTED")>0)
//                        CloseReader();
//                    break;
//            }
        //}

        /// <summary>
        /// This function process any tag that is returned as an event.
        /// This function is in use when you send a READ with REPORT=EVENT
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="EvtArgs"></param>

        #endregion
        void brdr_EventHandlerTag(object sender, EVT_Tag_EventArgs EvtArgs)
        {
            //bool bStatus = false;
            //txtTagID.Text=EvtArgs.DataString.ToString();
        }
        
        //new in v3.20
        void brdr_ReaderEventHandler(object sender, BasicReaderEventArgs EvtArgs)
        {
            System.Diagnostics.Debug.WriteLine("ReaderEvent: Type=" + EvtArgs.EventType.ToString() +
                "\n\tEventData='" + EvtArgs.EventData + "'");
            switch ((int)EvtArgs.EventType)
            {
                /*            
                    OLD:    UNKNOWN = 1,
                            SHUTDOWN = 2,
                            BUTTON = 3,
                            DEVICE = 4,
                    EventTypes.EVT_BATTERY
                    EventTypes.EVT_CENTER_TRIGGER
        EventTypes.EVT_DCE
Battery pulled
                 * ReaderEvent: Type=EVT_DCE
	                    EventData='DEVICE 1 DISCONNECTED'
                    ReaderEvent: Type=EVT_DCE
	                    EventData='DEVICE 1 CONNECTING'
                    The thread 0xc66b4f1a has exited with code 0 (0x0).
                    ReaderEvent: Type=EVT_DCE
	                    EventData='DEVICE 1 CONNECTED'
Center Trigger
                 * ReaderEvent: Type=EVT_TRIGGER
	                    EventData='TRIGPULL GPIO 0'
                    ReaderEvent: Type=EVT_DCE
	                    EventData='BUTTON CENTER 1'
                    ReaderEvent: Type=EVT_TRIGGER
	                    EventData='TRIGRELEASE GPIO 1'
                    ReaderEvent: Type=EVT_DCE
	                    EventData='BUTTON CENTER 0'

                    EventTypes.EVT_RADIO
                    EventTypes.EVT_READER_RECONNECTED
                    EventTypes.EVT_RESET
                    EventTypes.EVT_TAG
                    EventTypes.EVT_THERMAL
                    EventTypes.EVT_TRIGGER
                    EventTypes.EVT_TRIGGERACTION
                */
                case (int)BasicReaderEventArgs.EventTypes.EVT_RADIO:
                    brdr_EventHandlerRadio(sender, EvtArgs);
                    break;
                case (int)BasicReaderEventArgs.EventTypes.EVT_DCE: // EVT_CENTER_TRIGGER:
                    if (m_bEventLogging)
                        Add2List("brdr_ReaderEvent() BUTTON:" + EvtArgs.EventData);
                        brdr_EventHandlerCenterTrigger(sender, EvtArgs);
                    break;
                case (int)BasicReaderEventArgs.EventTypes.EVT_BATTERY:
                case (int)BasicReaderEventArgs.EventTypes.EVT_CENTER_TRIGGER:
                case (int)BasicReaderEventArgs.EventTypes.EVT_READER_RECONNECTED:
                case (int)BasicReaderEventArgs.EventTypes.EVT_RESET:
                case (int)BasicReaderEventArgs.EventTypes.EVT_TAG:
                case (int)BasicReaderEventArgs.EventTypes.EVT_THERMAL:
                case (int)BasicReaderEventArgs.EventTypes.EVT_TRIGGER:
                case (int)BasicReaderEventArgs.EventTypes.EVT_TRIGGERACTION:
                case (int)BasicReaderEventArgs.EventTypes.EVT_UNKNOWN:
                    if (m_bEventLogging)
                    {
                        Add2List("brdr_ReaderEvent() type: " + Convert.ToString((Int16)EvtArgs.EventType) + "=" + EvtArgs.EventType.ToString());
                        Add2List("brdr_ReaderEvent() data: '" + EvtArgs.EventData + "'");
                    }
                    break;
                //case 2:
                //    // The DCE is shutting down...
                //    if (m_bEventLogging)
                //        Add2List("ReaderEvent evt: DCE ShutDown");
                //    CloseReader();
                //    break;
                //case 4:
                //    if (m_bEventLogging)
                //        Add2List("brdr_ReaderEvent() DEVICE:" + EvtArgs.EventData);
                //        brdr_EventHandlerRadio(sender, EvtArgs);
                //    //if (EvtArgs.EventData.IndexOf("DISCONNECTED") > 0)
                //    //    CloseReader();
                //    break;
                default:
                    // The DCE has encountered an undefined condition...
                    if (m_bEventLogging)
                        Add2List("brdr_ReaderEvent(): Unknown Type " + Convert.ToString((Int16) EvtArgs.EventType) + " - Data: '" + EvtArgs.EventData + "'");
                    //CloseReader();
                    break;
            }
        }

        /// <summary>
        /// This function fires when the center trigger on the IP4 is pulled or released
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="EvtArgs"></param>
        void brdr_EventHandlerCenterTrigger(object sender, BasicReaderEventArgs EvtArgs)
        {
            if (m_Reader.IsConnected == false)
            {
                //irda connection is still asleep after a 700 resume
                ChangeStatus(eStatus.Offline);
                return;
            }
            //avoid multiple processing
            if (m_bCenterEventProcessing)
                return;
            m_bCenterEventProcessing = true;
            /*CenterTriggerState*/  /*EVT_CenterTrigger_EventArgs.STATE.PULLED*/
            if (EvtArgs.EventType.Equals(BasicReaderEventArgs.EventTypes.EVT_DCE) && // BasicReaderEventArgs.EventTypes.EVT_CENTER_TRIGGER) && 
                EvtArgs.EventData.Equals("BUTTON CENTER 1"))
            {
#region Write Single TAG
                if (((CurrentStatus == eStatus.WriteTag) || (CurrentStatus == eStatus.WriteTagError)) 
                    && !m_bEnableTwoTAGscanning )
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
                        vwTag.sDataID = sDataID;
                        vwTag.iSupplierID = UInt32.Parse(sSupplierID);
                        vwTag.iPackageNumber = UInt32.Parse(sPackageNumber);
                        if (m_bEditingEnabled )
                        {
                            if (CheckInput() == true)
                            {
                                vwTag.iBOXidentification = UInt32.Parse(txtBoxID.Text);
                                vwTag.iFilter = UInt32.Parse(txtFilter.Text);
                            }
                            else
                            {
                                m_bCenterEventProcessing = false;
                                return;
                            }
                        }
                        sHexCurrentEPCID = vwTag.HexData;
                    }
                    catch (SystemException sx)
                    {
                        Add2List("Error in building TagData" + sx.Message);
                        ChangeStatus(eStatus.InvalidTag);
                        m_bCenterEventProcessing = false;
                        return;
                    }
                    //"34AC2FA98811D6F345400001"
                    //Binary: 001101001010110000101111101010011000100000010001110101101111001101000101010000000000000000000001
                    //string: 100001000001000100010100000000010001010111011001100100001101101000011010101000101100100011101100
                    //5J UN 04-997-7473 123456789
                    // according to the scheme selected for the VW GTL EPC tag, the following is NOT
                    // really unique. TAGID is NOT unique to a tag
                    // as EPC-HEADER, Filter, Partition and Box identification dont make up a
                    // hex value (34 Bit), it is not suitable to use as unique identifier
                    // if it would be 32 or 40 bits, it could be used as WHERE filter
                    string sBitMask="";
                    VWGTLTAG2 oldTAG = new VWGTLTAG2(sOldTagHex);
                    sBitMask = tools.Uint2BinStr(oldTAG.iBOXidentification, 20) +
                        VWGTLTAG2.sBinStrPartition +
                        tools.Uint2BinStr(oldTAG.iFilter, 3) +
                        VWGTLTAG2.sBinStrEPCHeader;
                    
                    sBitMask = tools.ReverseString(sBitMask);
                    //H3418000040BEA620475BCD15
                    //B00110100 11011000 01111000 10010000 00=HD361E240
                    //B00110100110110000111100010010000 = H34D87890
                    //bits 32-128 are EPCID, EPCID memory bank bits 0-15 are CRC, 16-31 are PC
                    sBitMask = "BIT(1:32,34)=B" + sBitMask;
                    //where BIT does not work correctly with IP4, if IP4 model A or BRI version before S
                    if (m_bUseBitMask)
                        sCurrentCMD = "W TAGID EPCID=H" + sHexCurrentEPCID + " WHERE " + sBitMask;
                    else
                        sCurrentCMD = "W TAGID EPCID=H" + sHexCurrentEPCID + " WHERE TAGID=H" + sHexCurrentTagID;
                    //sCurrentCMD = "W TAGID EPCID=H" + sHexCurrentEPCID + " WHERE EPCID=H" + sOldTagHex;
                    if (ExecuteCMD(sCurrentCMD))
                    {
                        Add2List("OK");
                    }
                    else
                    {
                        Add2List("failed");
                    }
                }
#endregion
                else if (((CurrentStatus == eStatus.WriteTwoTags ) || (CurrentStatus == eStatus.WriteTwoTagsError ))
                    && m_bEnableTwoTAGscanning)
                {
                    //write two TAGs with same data except for TAG number
                    Add2List("Writing two TAGs...");
                    //save old EPCIDs
                    sHexCurrentEPCID = vwTag.HexData;
                    sHexCurrentEPCID2 = vwTag2.HexData;
                    //change data
                    vwTag.sDataID = sDataID;
                    vwTag.iSupplierID = UInt32.Parse(sSupplierID);
                    vwTag.iPackageNumber = UInt32.Parse(sPackageNumber);
                    //change data2
                    vwTag2.sDataID = sDataID;
                    vwTag2.iSupplierID = UInt32.Parse(sSupplierID);
                    vwTag2.iPackageNumber = UInt32.Parse(sPackageNumber);

                    tags[0].TagFields.FieldArray[0].Data = vwTag.Byte;// sByte;
                    tags[1].TagFields.FieldArray[0].Data = vwTag2.Byte; // sByte;
                    Add2List("TAG1:" + vwTag.ToString());
                    Add2List("TAG2:" + vwTag2.ToString());

                    // For BitMask usage
                    VWGTLTAG2 oldTAG = new VWGTLTAG2(vwTag.HexData);
                    VWGTLTAG2 oldTAG2 = new VWGTLTAG2(vwTag2.HexData);
                    string sBitMask = "";
                    string sBitMask2 = "";
                    sBitMask = tools.Uint2BinStr(oldTAG.iBOXidentification, 20) +
                        VWGTLTAG2.sBinStrPartition +
                        tools.Uint2BinStr(oldTAG.iFilter, 3) +
                        VWGTLTAG2.sBinStrEPCHeader;
                    sBitMask2 = tools.Uint2BinStr(oldTAG2.iBOXidentification, 20) +
                        VWGTLTAG2.sBinStrPartition +
                        tools.Uint2BinStr(oldTAG2.iFilter, 3) +
                        VWGTLTAG2.sBinStrEPCHeader;

                    sBitMask = tools.ReverseString(sBitMask);
                    sBitMask2 = tools.ReverseString(sBitMask2);
                    //H3418000040BEA620475BCD15
                    //B00110100 11011000 01111000 10010000 00=HD361E240
                    //B00110100110110000111100010010000 = H34D87890
                    //bits 32-128 are EPCID, EPCID memory bank bits 0-15 are CRC, 16-31 are PC
                    sBitMask = "BIT(1:32,34)=B" + sBitMask;
                    sBitMask2 = "BIT(1:32,34)=B" + sBitMask2;

                    //where BIT does not work correctly with IP4, if IP4 model A or BRI version before S
                    if (m_bUseBitMask)
                        sCurrentCMD = "W TAGID EPCID=H" + vwTag.HexData + " WHERE " + sBitMask;
                    else
                        sCurrentCMD = "W TAGID EPCID=H" + vwTag.HexData + " WHERE EPCID=H" + sOldTagHex;

                    //try every TAG only once
                    //sCurrentCMD = "W TAGID EPCID=H" + vwTag.HexData + " WHERE EPCID=H" + sOldTagHex;
                    if (!m_bTAG1writeOK)
                    {
                        System.Diagnostics.Debug.WriteLine("writing TAG1: \t" + sCurrentCMD);
                        m_bTAG1writeOK = ExecuteCMD(sCurrentCMD);
                        if (m_bTAG1writeOK)
                            Add2List("Writing TAG1 OK");
                        else
                            Add2List("Writing TAG1 failed");
                    }
                    //pause a little bit to avoid duty cycle failures
                    Add2List("Sleeping 500ms...");
                    System.Threading.Thread.Sleep(500);
                    Add2List("...OK");

                    if (m_bUseBitMask)
                        sCurrentCMD = "W TAGID EPCID=H" + vwTag2.HexData + " WHERE " + sBitMask2;
                    else
                        sCurrentCMD = "W TAGID EPCID=H" + vwTag2.HexData + " WHERE EPCID=H" + sOldTagHex2;

                    //sCurrentCMD = "W TAGID EPCID=H" + vwTag2.HexData + " WHERE EPCID=H" + sOldTagHex2;
                    if (!m_bTAG2writeOK)
                    {
                        System.Diagnostics.Debug.WriteLine("writing TAG2: \t" + sCurrentCMD);
                        m_bTAG2writeOK = ExecuteCMD(sCurrentCMD);
                        if (m_bTAG2writeOK)
                            Add2List("Writing TAG2 OK.");
                        else
                            Add2List("Writing TAG2 failed");
                    }
                    Add2List("Sleeping 500ms...");
                    System.Threading.Thread.Sleep(500);
                    Add2List("...OK");

                    if (m_bTAG1writeOK && m_bTAG2writeOK)
                    {
                            //except for two TAGs mode, this time change status from this code
                            ChangeStatus(eStatus.ReadBarcode);
                    }

                    //if (m_Reader.IsConnected)
                    //{
                    //    try
                    //    {
                    //        BRIReader m_BriReader = new BRIReader(this);
                    //        m_BriReader.AddTags(tags);
                    //        if (m_BriReader.Update())
                    //            Add2List("BriReader.Update() OK");
                    //        else
                    //            Add2List("BriReader.Update()=false");
                    //    }
                    //    catch (BRIParserException bx)
                    //    {
                    //        Add2List("Exception: " + bx.Message + " in WriteTwoTAGs");
                    //    }

                    //}
                    //else
                    //    Add2List("BriReader not connected");
                }
                else if (CurrentStatus == eStatus.ReadTwoTags | CurrentStatus == eStatus.ReadTwoTagsError)
                {
                    sCurrentCMD = "READ EPCID"; //TAGID
                    if (ExecuteCMD(sCurrentCMD))
                    {
                        Add2List("OK");
                    }
                    else
                    {
                        Add2List("failed");
                    }
                }
                else if (CurrentStatus == eStatus.ReadTag | CurrentStatus == eStatus.InvalidTag | CurrentStatus == eStatus.TagReadError)
                {
                    sCurrentCMD = "READ TAGID"; //TAGID
                    if (ExecuteCMD(sCurrentCMD))
                    {
                        Add2List("OK");
                    }
                    else
                    {
                        Add2List("failed");
                    }
                }

                else if (CurrentStatus == eStatus.TooManyTags)
                {
                    sCurrentCMD = "READ TAGID"; //"READ TAGID"
                    if (ExecuteCMD(sCurrentCMD))
                    {
                        Add2List("OK");
                    }
                    else
                    {
                        Add2List("failed");
                    }
                }
                else if (CurrentStatus == eStatus.ReadBarcode || CurrentStatus == eStatus.BarCodeErr ||
                            CurrentStatus == eStatus.BarcodeBoxIDError || CurrentStatus == eStatus.ReadBarcodeBoxID)
                    m_BCreader.ScannerOn = true;

            }
            /****/      else if (EvtArgs.EventType.Equals(BasicReaderEventArgs.EventTypes.EVT_DCE) && //EVT_CENTER_TRIGGER
                EvtArgs.EventData.Equals("BUTTON CENTER 0"))
                // CenterTriggerState //EVT_CenterTrigger_EventArgs.STATE.RELEASED))
            {
                if (CurrentStatus == eStatus.ReadBarcode || CurrentStatus == eStatus.BarCodeErr ||
                    CurrentStatus == eStatus.BarcodeBoxIDError || CurrentStatus == eStatus.ReadBarcodeBoxID)
                    m_BCreader.ScannerOn = false;
                //StopRead();
            }
            m_bCenterEventProcessing = false;
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
            byte[] aFieldSeparator ={ (byte)' ' };
            byte[] aRespBuf;
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
            if (tMsg.IndexOf("BRI ERR")>=0)
                System.Diagnostics.Debugger.Break();
            //"H112233445566778899001122 HE2001040\r\nOK>"
            Add2List("--->" + tMsg);
            //Show TAG ID
#region READ EPCID/TAGID
            if (sCurrentCMD.Equals("READ EPCID") || sCurrentCMD.Equals("READ TAGID")) //TAGID
            {
                if (tMsg.IndexOf("RDERR") >= 0)
                {
                    ChangeStatus(eStatus.TagReadError);
                    return false;
                }
                aRespBuf = tools.StrToByteArray(tMsg);
                tags = BRIParser.GetTags(aRespBuf, aFieldSeparator);
                if (tags == null)
                {
                    if (CurrentStatus == eStatus.ReadTag)
                        ChangeStatus(eStatus.TagReadError);
                    else
                        LowBeep2.Play();
                    return false;
                }
                //tags[] has the tag data
                //tags[0]=24 hex chars
                //tags[1]=24 hex chars
                if ((tags.GetLength(0) == 2) && (m_bEnableTwoTAGscanning == true))
                {
                    if ((VWGTLTAG2.IsValidTag(tags[0].ToString()) == false) || (VWGTLTAG2.IsValidTag(tags[1].ToString()) == false))
                    {
                        ChangeStatus(eStatus.ReadTwoTagsError);
                        return false;
                    }
                    else
                    {
                        sOldTagHex = tags[0].ToString();
                        vwTag = new VWGTLTAG2(sOldTagHex); //create a new tag with the tag data read
                        sOldTagHex2 = tags[1].ToString();
                        vwTag2 = new VWGTLTAG2(sOldTagHex2); //create a new tag with the tag data read
                        System.Diagnostics.Debug.WriteLine("Read TAG1: \t" + sOldTagHex + "\nBoxID:\t" + vwTag.iBOXidentification.ToString() + "\nTagID:\t" + vwTag.iFilter.ToString());
                        System.Diagnostics.Debug.WriteLine("Read TAG2: \t" + sOldTagHex2 + "\nBoxID:\t" + vwTag2.iBOXidentification.ToString() + "\nTagID:\t" + vwTag2.iFilter.ToString());
                        System.Diagnostics.Debug.WriteLine("Read vwTAG1: \t" + vwTag.HexData);
                        System.Diagnostics.Debug.WriteLine("Read vwTAG2: \t" + vwTag2.HexData);
                        txtTagData.Text = sOldTagHex;
                        txtTagID.Text = sOldTagHex2;
                        txtTagDataStr.Text = vwTag.ToString();
                        // Error checking
                        if (vwTag.iBOXidentification != vwTag2.iBOXidentification)
                        {
                            Add2List("Two TAGs: BoxID no match");
                            ChangeStatus(eStatus.ReadTwoTagsError);
                            return false;
                        }
                        if (vwTag.iFilter == vwTag2.iFilter)
                        {
                            Add2List("Two TAGs: same TAG number");
                            ChangeStatus(eStatus.ReadTwoTagsError);
                            return false;
                        }
                        //tags[0].TagFields.FieldArray[0].Data = vwTag.sByte;
                        //tags[1].TagFields.FieldArray[0].Data = vwTag2.sByte;
                        ChangeStatus(eStatus.WriteTwoTags);
                    }
                }
                else
                {
                    if (m_bEnableTwoTAGscanning)
                        ChangeStatus(eStatus.ReadTwoTagsError);
                }

#region READ single TAG
                if ((tags.GetLength(0) == 1) && (m_bEnableTwoTAGscanning==false))
                {
                    //only one tag should be visible
                    //save the last 22Bits as Box ID
                    if (VWGTLTAG2.IsValidTag(tags[0].ToString()) == false)
                    {
                        DialogResult ant = MessageBox.Show("Tag does not meet specs. Overwrite?", "VW tag demo", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                        if (ant == DialogResult.Yes)
                        {
                            //create a blank tag
                            vwTag = new VWGTLTAG2();
                            //store saved values to tag
                            vwTag.sDataID = sDataID;
                            vwTag.iBOXidentification = 0;
                            vwTag.iFilter = 0;
                            txtFilter.Text = vwTag.iFilter.ToString();
                            txtBoxID.Text = vwTag.iBOXidentification.ToString();
                            sOldTagHex = tags[0].ToString();
                        }
                        else
                        {
                            ChangeStatus(eStatus.InvalidTag);
                            return false;
                        }
                    }
                    else
                    {
                        sOldTagHex = tags[0].ToString();
                        vwTag = new VWGTLTAG2(tags[0].ToString()); //create a new tag with the tag data read
                    }
                    //read data from the tag
                    txtBoxID.Text = vwTag.iBOXidentification.ToString();
                    txtFilter.Text = vwTag.iFilter.ToString();
                    //fill txtbox with data from tag
                    txtTagDataStr.Text = vwTag.sDataID + " " +
                                    vwTag.iSupplierID.ToString().PadLeft(9,'0') + " " +
                                    vwTag.iPackageNumber.ToString().PadLeft(9, '0');

                    System.Diagnostics.Debug.WriteLine("READ TAGID tag data: " + vwTag.ToString());
                    //tagKey is the same as tag data
                    //System.Diagnostics.Debug.WriteLine("READ TAGID: tags[0].tagkey: " + tools.Hex2Asc(sByteArrayToString(tags[0].TagKey)));

                    //store TAGID
                    sHexCurrentTagID = tags[0].TagFields.FieldArray[0].DataString.Substring(1) ;

                    if (m_bBoxIDscanningEnabled) //added with v3
                        ChangeStatus(eStatus.ReadBarcodeBoxID);
                    else
                    {
                        if (m_bEnableTwoTAGscanning)
                            ChangeStatus(eStatus.WriteTwoTags);
                        else
                            ChangeStatus(eStatus.WriteTag);
                    }
                    //ver 2.1: ChangeStatus(eStatus.WriteTag);
                }
                else
                {
                    //only one tag should be visible
                    if (!m_bEnableTwoTAGscanning)
                        ChangeStatus(eStatus.TooManyTags);
                }
#endregion
                //read tag data (aka EPCID)
                if (tMsg.StartsWith("H") && (m_bEnableTwoTAGscanning == false))
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
#endregion
#region ATTRIB
            if (sCurrentCMD.Equals("ATTRIB TAGTYPE=EPCC1G2") == true)
            {
                if (tMsg.IndexOf("OK") >= 0)
                    return true;
                else
                    return false;
            }
            if ((tMsg.IndexOf("ERR") >= 0) & (sCurrentCMD.Equals("ATTRIB SCHEDOPT=1") == false))
            {
                //MessageBox.Show("Warning, BRI ERR occured for: " + tCMD + "\r\n" + tMsg);
                Add2List("BRI ERR for: " + tCMD + "\r\n" + tMsg);
                if (CurrentStatus == eStatus.ReadTag)
                    ChangeStatus(eStatus.TagReadError);
                else if (CurrentStatus == eStatus.WriteTag)
                    ChangeStatus(eStatus.WriteTagError);
                else
                    LowBeep2.Play();
                return false;
            }
#endregion
#region W TAG single TAG writing
            if (sCurrentCMD.StartsWith("W "))
            {
                aRespBuf = tools.StrToByteArray(tMsg);
                if (BRIParser.IsWriteError(aRespBuf))
                {
                    if (m_bEnableTwoTAGscanning)
                        ChangeStatus(eStatus.WriteTwoTagsError);
                    else
                        ChangeStatus(eStatus.WriteTagError);
                    return false;
                }
                if (tMsg.IndexOf("WRERR") >= 0)
                {
                    if (m_bEnableTwoTAGscanning)
                        ChangeStatus(eStatus.WriteTwoTagsError);
                    else
                        ChangeStatus(eStatus.WriteTagError);
                    return false;
                }
                //if (BRIParser.IsWriteOK(aRespBuf) & (CurrentStatus == eStatus.WriteTagError) || (CurrentStatus == eStatus.WriteTag))
                if (tMsg.IndexOf("WROK") >= 0)
                //if (tMsg.IndexOf("WROK") >= 0 & (CurrentStatus == eStatus.WriteTagError) | (CurrentStatus == eStatus.WriteTag))
                {
                    HighBeep.Play();
                    if (!m_bEnableTwoTAGscanning)
                        ChangeStatus(eStatus.ReadBarcode);
                    if (m_bDebugModus)
                    {
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
                            txtTagID.Text = s.Substring(0, s.IndexOf(" ") - 1);
                            System.Diagnostics.Debug.WriteLine(s);
                        }
                    }
                    return true;
                }
                else
                {
                    if (m_bEnableTwoTAGscanning)
                        ChangeStatus(eStatus.WriteTwoTagsError);
                    else
                        ChangeStatus(eStatus.WriteTagError);
                    return false;
                }
            }//W command
            return true;
#endregion
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
                if (tChar.Equals('\n') == false & tChar.Equals('\r') == false)
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
                if (RspMsgList[x].IndexOf("H") == 0 & RspMsgList[x].IndexOf("HOP") < 0)
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
                    if (sCurrentCMD.StartsWith("R") == false & sCurrentCMD.StartsWith("W") == false)
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
                m_BCreader.symbology.DisableAll();
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
            if ((iSymID == SymbologyLicensePlate) && 
                (CurrentStatus == eStatus.ReadBarcode || CurrentStatus == eStatus.BarCodeErr ))
            {
                sBarcode = bre.strDataBuffer;
                //remove unneeded chars
                sBarcode = sBarcode.Replace(" ", "");
                sBarcode = sBarcode.Replace("-", "");
                //test the barcode
                if (!VWGTLTAG2.CheckBarCode(sBarcode))
                {
                    Add2List(VWGTLTAG2.LastError);
                    ChangeStatus(eStatus.BarCodeErr);
                    return;
                }
                //store data
                sDataID = sBarcode.Substring(0, 4);
                sSupplierID = sBarcode.Substring(4, 9);
                sPackageNumber = sBarcode.Substring(13, 9);
                System.Diagnostics.Debug.WriteLine("Barcode Data:\nFilter: " + sDataID + "\nSupplierID: " + sSupplierID + "\nItemNumber: " + sPackageNumber);

                //set the barcode data
                if (m_bEnableTwoTAGscanning)
                    ChangeStatus(eStatus.ReadTwoTags);
                else
                    ChangeStatus(eStatus.ReadTag);
                txtBarcodeData.Text = sBarcode;
                txtTagID.Text = "";
                txtTagData.Text = "";
                txtTagDataStr.Text = "";
            }
            else if ((iSymID == SymbologyBoxID) && 
                        (CurrentStatus == eStatus.ReadBarcodeBoxID || CurrentStatus==eStatus.BarcodeBoxIDError ) )
            {
                if (m_bBoxIDscanningEnabled )
                {
                    UInt32 BoxId = 0;
                    int LabelNr = 0;
                    string tmp = "";
                    try
                    {
                        tmp = bre.strDataBuffer;
                        if (tmp.Length != 5)
                        {
                            ChangeStatus(eStatus.BarcodeBoxIDError);
                            System.Diagnostics.Debug.WriteLine("BoxID scan is not 5 digits!");
                            Add2List("BoxID scan is not 5 digits!");
                            throw new ArgumentOutOfRangeException();
                        }
                        BoxId = Convert.ToUInt32(tmp.Substring(1));
                        if (BoxId > 1048576) //2^20=1048576
                        {
                            ChangeStatus(eStatus.BarcodeBoxIDError);
                            System.Diagnostics.Debug.WriteLine("BoxID scan exceeds 2^20");
                            Add2List("BoxID scan exceeds 2^20");
                            throw new ArgumentOutOfRangeException();
                        }
                        LabelNr=Convert.ToInt16(tmp.Substring(0,1));
                        //LabelNs is 1 for TAG 1a and 8 for TAG 1b
                        if (LabelNr == 8)
                            LabelNr = 2;
                    }
                    catch (SystemException  ex)
                    { 
                        System.Diagnostics.Debug.WriteLine("BoxID scan exception:" + ex.Message );
                        BoxId = 0;
                        LabelNr = 0;
                    }
                    iCurrentBoxID = BoxId;
                    txtBoxID.Text = BoxId.ToString("0000");
                    txtFilter.Text = LabelNr.ToString();
                    if (BoxId == 0 || LabelNr == 0)
                        ChangeStatus(eStatus.BarcodeBoxIDError);
                    else
                    {
                        if (m_bEnableTwoTAGscanning)
                            ChangeStatus(eStatus.WriteTwoTags);
                        else
                            ChangeStatus(eStatus.WriteTag);
                    }
                }
            }

        }
        private void ConnectReader()
        {
            Cursor.Current = Cursors.WaitCursor;
            System.Diagnostics.Debug.WriteLine("Opening Reader...");
            if (m_BCreader == null)
                OpenBCreader();
            if (OpenReader() == 0)
            {
                System.Diagnostics.Debug.WriteLine("... Reader opened. Trying to set Attributes ...");
                //try first command
                int res = SetAttributes();
                if (res != 0)
                {
                    System.Diagnostics.Debug.WriteLine("... SetAttributes failed! Closing Reader");
                    m_Reader.Close();
                    m_Reader = null;
                }
            }

            if (m_Reader != null)
            {
                if (m_Reader.IsConnected)
                {
                    System.Diagnostics.Debug.WriteLine("Reader connected. ChangeStatus(ReadBarcode)");
                    ChangeStatus(eStatus.ReadBarcode);//  ReadTag);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Reader connected. ChangeStatus(ReadBarcode)");
                    ChangeStatus(eStatus.Offline);
                }
            }
            else
                ChangeStatus(eStatus.Offline);

            Cursor.Current = Cursors.Default;
        }
        private void mnuConnectReader_Click(object sender, EventArgs e)
        {
            ConnectReader();
        }

        private void CloseReader()
        {
            if (m_Reader != null)
                try
                {
                    if (m_Reader != null)
                        m_Reader.Close();
                    RemoveEventHandlers();
                    btnConnect.Text = "Connect Reader";
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
            mnuEnableBRILogging.Checked = !mnuEnableBRILogging.Checked;
            if (mnuEnableBRILogging.Checked)
                bEnableLogging = true;
            else
                bEnableLogging = false;
        }

        private void mnuWriteTag_Click(object sender, EventArgs e)
        {
            if (m_Reader == null) 
            {
                ChangeStatus(eStatus.Offline);
                return;
            }
            if (!m_Reader.IsConnected)
            {
                ChangeStatus(eStatus.Offline);
                return;
            }

            if (CheckInput() == false)
            {
                LowBeep2.Play();
                return;
            }
            try
            {
                //change data
                vwTag.sDataID = sDataID;
                vwTag.iSupplierID = UInt32.Parse(sSupplierID);
                vwTag.iPackageNumber = UInt32.Parse(sPackageNumber);
                vwTag.iFilter = UInt32.Parse(txtFilter.Text);
                vwTag.iBOXidentification = UInt32.Parse(txtBoxID.Text);
                sHexCurrentEPCID = vwTag.HexData;
            }
            catch
            {
                LowBeep2.Play();
                Add2List("mnuWriteTAG: Error in evaluating values");
                return;
            }
            if (MessageBox.Show("Ready to write H" + sHexCurrentEPCID, "Manual Tag Write", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
                return;
            sCurrentCMD = "W TAGID EPCID=H" + sHexCurrentEPCID ;

            if ((m_Reader != null) && (m_Reader.IsConnected))
            {
                if (m_bBoxIDscanningEnabled)
                    ChangeStatus(eStatus.ReadBarcodeBoxID);
                else
                {
                    if (m_bEnableTwoTAGscanning)
                        ChangeStatus(eStatus.WriteTwoTags);
                    else
                        ChangeStatus(eStatus.WriteTag);
                    ExecuteCMD(sCurrentCMD);
                }
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
            if (m_Reader != null)
            {
                if (m_Reader.IsConnected)
                {
                    sCurrentCMD = "READ TAGID";//TAGID
                    if (m_bEnableTwoTAGscanning)
                        ChangeStatus(eStatus.ReadTwoTags);
                    else
                        ChangeStatus(eStatus.ReadTag);
                    ExecuteCMD(sCurrentCMD);
                }
                else
                    Add2List("Reader not connected!");
            }
            else
                Add2List("Reader not available. Please Connect");
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

        private void mnuEventLogging_Click(object sender, EventArgs e)
        {
            mnuEventLogging.Checked = !mnuEventLogging.Checked;
            m_bEventLogging = mnuEventLogging.Checked;
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
            mnuEditingEnabled.Checked = !mnuEditingEnabled.Checked;
            EnableEditing(mnuEditingEnabled.Checked );
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("ip4 Scan-N-tag\nDemo-Application\nVersion 3.1", this.Text);
            string v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string t = "ip4 Scan-N-tag\nDemo-Application\nVersion " + v;
            MessageBox.Show(t, this.Text);
        }

        private void mnuUseBitMask_Click(object sender, EventArgs e)
        {
            mnuUseBitMask.Checked = !mnuUseBitMask.Checked;
            m_bUseBitMask = mnuUseBitMask.Checked;
        }

        private void mnu_BoxIDscanning_Click(object sender, EventArgs e)
        {
            mnu_BoxIDscanning.Checked = !mnu_BoxIDscanning.Checked;
            m_bBoxIDscanningEnabled = mnu_BoxIDscanning.Checked;
        }
        private void WriteIni()
        {
            // Create a RegistryKey, which will access the HKEY_USERS
            // key in the registry of this machine.
            const string subkey = "SOFTWARE\\INTERMEC\\ip4scanNtag";
            try
            {

                RegistryKey rk = Registry.LocalMachine;
                rk.CreateSubKey(subkey);
                rk = Registry.LocalMachine.OpenSubKey(subkey, true);

                if (m_bBoxIDscanningEnabled )
                    rk.SetValue("BoxIDscanningEnabled", 1);
                else
                    rk.SetValue("BoxIDscanningEnabled", 0);
                
                if (m_bEditingEnabled )
                    rk.SetValue("EditingEnabled", 1);
                else
                    rk.SetValue("EditingEnabled", 0);
                
                if (m_bEventLogging )
                    rk.SetValue("EventLogging", 1);
                else
                    rk.SetValue("EventLogging", 0);
                
                if (m_bFreeInput  )
                    rk.SetValue("FreeInput", 1);
                else
                    rk.SetValue("FreeInput", 0);

                if (m_bUseBitMask)
                    rk.SetValue("UseBitMask", 1);
                else
                    rk.SetValue("UseBitMask", 0);
                //new in v3
                if (m_bEnableTwoTAGscanning)
                    rk.SetValue("EnableTwoTAGscanning", 1);
                else
                    rk.SetValue("EnableTwoTAGscanning", 0);

                rk.Close();
            }
            catch (SystemException sx)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    MessageBox.Show(sx.Message, "WriteIni()");
            }
        }
        private void ReadIni()
        {
            const string subkey = "SOFTWARE\\INTERMEC\\ip4scanNtag";
            RegistryKey rk = Registry.LocalMachine.OpenSubKey(subkey);
            int ret = 0;
            try
            {
                ret = (int)rk.GetValue("BoxIDscanningEnabled", 0);
                if (ret == 1)
                    m_bBoxIDscanningEnabled = true;
                else
                    m_bBoxIDscanningEnabled = false;
                mnu_BoxIDscanning.Checked = m_bBoxIDscanningEnabled;

                ret = (int)rk.GetValue("EditingEnabled", 0);
                if (ret == 1)
                    m_bEditingEnabled = true;
                else
                    m_bEditingEnabled = false;
                mnuEditingEnabled.Checked = m_bEditingEnabled;

                ret = (int)rk.GetValue("EventLogging", 0);
                if (ret == 1)
                    m_bEventLogging = true;
                else
                    m_bEventLogging = false;
                mnuEventLogging.Checked = m_bEventLogging;

                ret = (int)rk.GetValue("FreeInput", 0);
                if (ret == 1)
                    m_bFreeInput = true;
                else
                    m_bFreeInput = false;
                mnuFreeCMDs.Checked = m_bFreeInput;
                //UseBitMask
                ret = (int)rk.GetValue("UseBitMask", 0);
                if (ret == 1)
                    m_bUseBitMask = true;
                else
                    m_bUseBitMask = false;
                mnuUseBitMask.Checked = m_bUseBitMask;
                //EnableTwoTAGscanning
                ret = (int)rk.GetValue("EnableTwoTAGscanning", 0);
                if (ret == 1)
                    m_bEnableTwoTAGscanning = true;
                else
                    m_bEnableTwoTAGscanning = false;
                mnuEnableTwoTAGscanning.Checked = m_bEnableTwoTAGscanning;
                EnableTwoTAGsMode(m_bEnableTwoTAGscanning);
            }
            catch (SystemException sx)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    MessageBox.Show(sx.Message, "ReadIni()");
            }
        }

        private void menuItem2_Click(object sender, EventArgs e)
        {
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                btnConnect.Enabled = false;
                Cursor.Current = Cursors.WaitCursor;
                if (CurrentStatus == eStatus.Offline)
                    ConnectReader();
                else
                    mnuDisconnectReader_Click(sender, e);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
                btnConnect.Enabled = true;
            }
        }

        private void mnuEnableOptions_Click(object sender, EventArgs e)
        {
            if (menuItem2.Enabled)
                menuItem2.Enabled = false;
            else
            {
                //Enter Password
                PasswordDlg dlg = new PasswordDlg();
                dlg.ShowDialog();
                if (dlg.DialogResult == DialogResult.OK)
                    menuItem2.Enabled = true;
                else
                    menuItem2.Enabled = false;
                dlg.Dispose();
            }
        }

        private void mnuEnableTwoTAGscanning_Click(object sender, EventArgs e)
        {
            mnuEnableTwoTAGscanning.Checked = !mnuEnableTwoTAGscanning.Checked;
            m_bEnableTwoTAGscanning = mnuEnableTwoTAGscanning.Checked;
            EnableTwoTAGsMode(m_bEnableTwoTAGscanning);
        }
    }
}