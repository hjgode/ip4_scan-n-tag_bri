//=====================================================================================================
//
//	EPC Codec (Coder/Decoder)
//
//	5/24/06: Updated to EPCglobal Ratified TDS 1.1 Rev 1.27. Slight change to SSCC-96.
//	7/18/06: Added DOD-64 and DOD-96
//
//	Implements EPC conversions for 64- and 96-bit formats for SGTIN, SSCC, SGLN, GRAI, GIAI, DOD,
//	and GID.
//
//	Hal Endresen August 2005
//	Copyright (c) 2005 Intermec Technologies Corp. All rights reserved.
//	Proprietary and confidential property of Intermec Technologies Corp.
//
//	There are two EPC data type classes Epc64 and Epc96 which are joined by the interface IEpcCode.
//	The codec is made up of the primary Codec class and its component classes Field and Partition.
//
//	Because the codec operates on data types via an interface, there is no way to allocate a new
//	object as the codec does not know which data type it is operating on. For this reason the Epc64
//	and Epc96 classes both support an overloaded New() method which mimics the constructors and
//	avoids the use of reflection. The codec can then create new EPC data types via
//	Codec.EpcCode.New(...) and create the correct type.
//
//=====================================================================================================
using System;
using System.Text;
using System.Collections;

namespace Intermec.Demo
{
	public class Epc
	{
		/// <summary>
		/// Defines the field names (name enumeration) of the fields that can be represented
		/// </summary>
		public enum FieldID { Header, Filter, Partition, Company, Item, Location, Asset, Serial, GeneralManagerNo, ObjectClass, GovtManagedID }

		/// <summary>
		/// Defines the encoding formats
		/// </summary>
		public enum Format { GID96, SGTIN96, SGTIN64, SSCC96, SSCC64, SGLN96, SGLN64, GRAI96, GRAI64, GIAI96, GIAI64, DOD64, DOD96 }

		public Epc(){}

		/// <summary>
		/// 
		/// </summary>
		public interface IEpcCode : ICloneable
		{
			int				ByteLength { get; }
			int				BitLength { get; }
			IEpcCode		New();
			IEpcCode		New(string HexString);
			IEpcCode		New(byte[] ByteArray);
			void			Clear();
			UInt64			Field { get; set; }
			IEpcCode		ShiftLeft(int ShiftCount);
			IEpcCode		ShiftRight(int ShiftCount);
			IEpcCode		Or(IEpcCode Epc);
			IEpcCode		And(IEpcCode Epc);
			IEpcCode		Not();
			byte[]			GetBytes();
			string			ToHex();
		}

		public interface IEpcCodec
		{
			Epc.Format		Format {get; }
			int				BitLength { get; }
			int				ByteLength { get; }
			Epc.IEpcCode	EpcCode { get; }
			Epc.Codec.Field	AddField(Epc.FieldID Type);
			int				Count { get; }
			string			Name {get;}
			int				Header { get;}
			bool			UsesField(FieldID FieldId);
			Epc.FieldID[]	ComponentFields { get; }
			Epc.Codec.Field	this[int index] { get; }
			Epc.Codec.Field	this[Epc.FieldID FieldType] { get; }
			int				FindPartitionValue();
			IEpcCode		Encode();
			void			Decode(IEpcCode epcInput);
			string			URN { get; }
		}

		#region EPC Decoder
		public class Decoder
		{
			private IEpcCodec	_sgtin96 = null;
			private IEpcCodec	_sgtin64 = null;
			private IEpcCodec	_sscc96 = null;
			private IEpcCodec	_sscc64 = null;
			private IEpcCodec	_sgln96 = null;
			private IEpcCodec	_sgln64 = null;
			private IEpcCodec	_grai96 = null;
			private IEpcCodec	_grai64 = null;
			private IEpcCodec	_giai96 = null;
			private IEpcCodec	_giai64 = null;
			private IEpcCodec	_gid96 = null;
			private IEpcCodec	_dod64 = null;
			private IEpcCodec	_dod96 = null;

			public Decoder(){}

			/// <summary>
			/// Decode EPC. This method accepts an EPC code and returns a codec that contains the
			/// decoded data. Null is returned if the EPC code is not valid.
			/// </summary>
			/// <param name="hexEpcCode"></param>
			/// <returns></returns>
			public IEpcCodec Decode(string hexEpcCode)
			{
				IEpcCodec codec = null;

				// make sure the header byte is present
				if (hexEpcCode.Length < 16)
					return null;

				// get the header byte
				byte headerByte = byte.Parse(new String(new Char[] { hexEpcCode[0], hexEpcCode[1] } ), System.Globalization.NumberStyles.HexNumber);

				// Find out which encoding it is. This requires stepping through the codecs because each one can
				// have a different header mask, so we call a static method on each codec. The checks are done in
				// order of expected frequency.
				if (sgtin96.MatchesHeader(headerByte))
				{
					if ((codec = this._sgtin96) == null)
						this._sgtin96 = codec = new sgtin96();
				}
				else if (sscc96.MatchesHeader(headerByte))
				{
					if ((codec = this._sscc96) == null)
						this._sscc96 = codec = new sscc96();
				}
				else if (sgln96.MatchesHeader(headerByte))
				{
					if ((codec = this._sgln96) == null)
						this._sgln96 = codec = new sgln96();
				}
				else if (grai96.MatchesHeader(headerByte))
				{
					if ((codec = this._grai96) == null)
						this._grai96 = codec = new grai96();
				}
				else if (giai96.MatchesHeader(headerByte))
				{
					if ((codec = this._giai96) == null)
						this._giai96 = codec = new giai96();
				}
				else if (dod96.MatchesHeader(headerByte))
				{
					if ((codec = this._dod96) == null)
						this._dod96 = codec = new dod96();
				}
				else if (gid96.MatchesHeader(headerByte))
				{
					if ((codec = this._gid96) == null)
						this._gid96 = codec = new gid96();
				}
				else if (sgtin64.MatchesHeader(headerByte))
				{
					if ((codec = this._sgtin64) == null)
						this._sgtin64 = codec = new sgtin64();
				}
				else if (sscc64.MatchesHeader(headerByte))
				{
					if ((codec = this._sscc64) == null)
						this._sscc64 = codec = new sscc64();
				}
				else if (sgln64.MatchesHeader(headerByte))
				{
					if ((codec = this._sgln64) == null)
						this._sgln64 = codec = new sgln64();
				}
				else if (grai64.MatchesHeader(headerByte))
				{
					if ((codec = this._grai64) == null)
						this._grai64 = codec = new grai64();
				}
				else if (giai64.MatchesHeader(headerByte))
				{
					if ((codec = this._giai64) == null)
						this._giai64 = codec = new giai64();
				}
				else if (dod64.MatchesHeader(headerByte))
				{
					if ((codec = this._dod64) == null)
						this._dod64 = codec = new dod64();
				}
				else
				{
					// unknown or invalid encoding
					return null;
				}

				// 'codec' now has the appropriate codec. Convert the hex EPC string to an EPC code and decode it.
				try
				{
					codec.Decode(codec.EpcCode.New(hexEpcCode));
				}
				catch
				{
					// invald encoding
					return null;
				}

				// and return the codec with the decoded fields
				return codec;
			}
		}
		#endregion
        
		#region Epc64
		/// <summary>
		/// Implements an EPC-64 data type
		/// </summary>
		public class Epc64 : IEpcCode, ICloneable
		{
			/// <summary>
			/// Number of bytes required on tag for this format
			/// </summary>
			public int	BitLength { get { return 64; } }					// public bit length for this format
			public int	ByteLength { get { return (this.BitLength/8); } }	// public byte length for this format

			private UInt64		epcCode;			// internal representation

			// create an Epc64 from an integer (not public)
			private Epc64(UInt64 number)
			{
				this.epcCode = number;
			}

			/// <summary>
			/// Create an Epc64 with all bits set to zero
			/// </summary>
			public Epc64()
			{
				this.epcCode = 0;
			}

			/// <summary>
			/// Create an Epc64 from a 16-character hex string
			/// </summary>
			/// <param name="HexString"></param>
			public Epc64(string HexString)
			{
				if (HexString.Length != this.ByteLength * 2)
					throw new ArgumentException(String.Format("Hex string must be {0} characters", this.ByteLength * 2));
			
				this.epcCode = 0;
				for (int x = 0; x < HexString.Length/2; x++)
				{
					this.epcCode <<= 8;
					this.epcCode += byte.Parse(new String(new Char[] { HexString[x*2], HexString[(x*2)+1] } ), System.Globalization.NumberStyles.HexNumber);
				}
			}

			/// <summary>
			/// Create an Epc64 from an 8-byte array
			/// </summary>
			/// <param name="Bytes"></param>
			public Epc64(byte[] Bytes)
			{
				if (Bytes.Length != this.ByteLength)
					throw new ArgumentException(String.Format("Byte array must be {0} bytes", this.ByteLength));
			
				this.epcCode = 0;
				for (int x = 0; x < Bytes.Length; ++x)
				{
					this.epcCode <<= 8;
					this.epcCode += Bytes[x];
				}
			}

			public IEpcCode ShiftLeft(int ShiftCount)
			{
				return (IEpcCode) new Epc64(this.epcCode << ShiftCount);
			}
			public static Epc64 operator << (Epc64 p1, int shiftCount)
			{
				return new Epc64(p1.epcCode << shiftCount);
			}

			public IEpcCode ShiftRight(int ShiftCount)
			{
				return (IEpcCode) new Epc64(this.epcCode >> ShiftCount);
			}
			public static Epc64 operator >> (Epc64 p1, int shiftCount)
			{
				return new Epc64(p1.epcCode >> shiftCount);
			}
		
			public IEpcCode Or(IEpcCode p1)
			{
				return (IEpcCode) new Epc64(((Epc64)p1).epcCode | this.epcCode);
			}
			public static Epc64 operator | (Epc64 p1, Epc64 p2)
			{
				return new Epc64(p1.epcCode | p2.epcCode);
			}

			public IEpcCode And(IEpcCode p)
			{
				return (IEpcCode) new Epc64(((Epc64)p).epcCode & this.epcCode);
			}
			public static Epc64 operator & (Epc64 p1, Epc64 p2)
			{
				return new Epc64(p1.epcCode & p2.epcCode);
			}

			public IEpcCode Not()
			{
				return (IEpcCode) new Epc64(~this.epcCode);
			}
			public static Epc64 operator ~ (Epc64 p)
			{
				return new Epc64(~p.epcCode);
			}

			public static explicit operator Epc64(int i)
			{
				return new Epc64((UInt64) i);
			}

			public void Clear()
			{
				this.epcCode = 0;
			}

			public IEpcCode New()
			{
				return (IEpcCode) new Epc64();
			}

			public IEpcCode New(string HexString)
			{
				return (IEpcCode) new Epc64(HexString);
			}

			public IEpcCode New(byte[] ByteArray)
			{
				return (IEpcCode) new Epc64(ByteArray);
			}

			public UInt64 Field
			{
				get { return this.epcCode; }
				set { this.epcCode = value; }
			}

			public byte[] GetBytes()
			{
				UInt64 temp = this.epcCode;

				// create an 8-byte array of zero
				byte[] byteArray = new byte[this.ByteLength];

				// shift the integer into the array
				for (int byteNum = byteArray.Length-1; byteNum >= 0; --byteNum)
				{
					byteArray[byteNum] = (byte) (temp & 0xff);
					temp >>= 8;
				}
				return byteArray;
			}

			public string ToHex()
			{
				UInt64 temp = this.epcCode;
				StringBuilder hex = new StringBuilder(this.ByteLength*2);
				for (int x = 0; x < this.ByteLength; ++x)
				{
					hex.Insert(0, (temp & 0xff).ToString("X2"));
					temp >>= 8;
				}
				return hex.ToString();
			}
			#region ICloneable Members
			public object Clone()
			{
				return new Epc64(this.epcCode);
			}
			#endregion
		}
		#endregion

		#region Epc96
		/// <summary>
		/// Implements an EPC-96 data type
		/// </summary>
		public class Epc96 : IEpcCode, ICloneable
		{
			public int	BitLength { get { return 96; } }					// public bit length for this format
			public int	ByteLength { get { return (this.BitLength/8); } }	// public byte length for this format
		
			private const int	BitLengthLowPart	= sizeof(UInt64)*8;		// number of bits in low part of two-integer representation
			
			private UInt64		ls64;										// LS part of 96-bit unsigned integer
			private UInt32		ms32;										// MS part of 96-bit unsigned integer

			// create an Epc96 from its two component parts (not public)
			private Epc96(UInt64 low, UInt32 high)
			{
				this.ls64 = low;
				this.ms32 = high;
			}

			/// <summary>
			/// Creates an Epc96 where all bits are zero.
			/// </summary>
			public Epc96()
			{
				this.Clear();
			}

			/// <summary>
			/// Creates an Epc96 from a 24-character hex string
			/// </summary>
			/// <param name="HexString"></param>
			public Epc96(string HexString)
			{
				if (HexString.Length != this.ByteLength * 2)
					throw new ArgumentException(String.Format("Hex string must be {0} characters in length", this.ByteLength * 2));
			
				this.Clear();
				for (int x = 0; x < this.ByteLength; x++)
				{
					Epc96.ShiftLeft8(ref this.ls64, ref this.ms32);
					this.ls64 |= byte.Parse(new String(new Char[] { HexString[x*2], HexString[(x*2)+1] } ), System.Globalization.NumberStyles.HexNumber);
				}
			}

			/// <summary>
			/// Creates an Epc96 from a 12-byte array
			/// </summary>
			/// <param name="Bytes"></param>
			public Epc96(byte[] Bytes)
			{
				if (Bytes.Length != this.ByteLength)
					throw new ArgumentException(String.Format("Byte array must be {0} bytes", this.ByteLength));
			
				this.Clear();
				for (int x = 0; x < Bytes.Length; ++x)
				{
					Epc96.ShiftLeft8(ref this.ls64, ref this.ms32);
					this.ls64 |= Bytes[x];
				}
			}

			public IEpcCode ShiftLeft(int ShiftCount)
			{
				Epc96 newepc = new Epc96(this.ls64, this.ms32);
				Epc96.ShiftLeft(ref newepc.ls64, ref newepc.ms32, ShiftCount);
				return (IEpcCode) newepc;
			}
			public static Epc96 operator << (Epc96 p1, int shiftCount)
			{
				Epc96 newepc = new Epc96(p1.ls64, p1.ms32);
				Epc96.ShiftLeft(ref newepc.ls64, ref newepc.ms32, shiftCount);
				return newepc;
			}

			public IEpcCode ShiftRight(int ShiftCount)
			{
				Epc96 newepc = new Epc96(this.ls64, this.ms32);
				Epc96.ShiftRight(ref newepc.ls64, ref newepc.ms32, ShiftCount);
				return (IEpcCode) newepc;
			}
			public static Epc96 operator >> (Epc96 p1, int shiftCount)
			{
				Epc96 newepc = new Epc96(p1.ls64, p1.ms32);
				Epc96.ShiftRight(ref newepc.ls64, ref newepc.ms32, shiftCount);
				return newepc;
			}
		
			public IEpcCode Or(IEpcCode p)
			{
				return (IEpcCode) new Epc96(this.ls64 | ((Epc96)p).ls64, this.ms32 | ((Epc96)p).ms32);
			}
			public static Epc96 operator | (Epc96 p1, Epc96 p2)
			{
				return new Epc96(p1.ls64 | p2.ls64, p1.ms32 | p2.ms32);
			}

			public IEpcCode And(IEpcCode p)
			{
				return (IEpcCode) new Epc96(this.ls64 & ((Epc96)p).ls64, this.ms32 & ((Epc96)p).ms32);
			}
			public static Epc96 operator & (Epc96 p1, Epc96 p2)
			{
				return new Epc96(p1.ls64 & p2.ls64, p1.ms32 & p2.ms32);
			}

			public IEpcCode Not()
			{
				return (IEpcCode) new Epc96(~this.ls64, ~this.ms32);
			}
			public static Epc96 operator ~ (Epc96 p)
			{
				return new Epc96(~p.ls64, ~p.ms32);
			}

			public static explicit operator Epc96(int i)
			{
				return new Epc96((UInt64) i, 0);
			}

			public IEpcCode New()
			{
				return (IEpcCode) new Epc96();
			}

			public IEpcCode New(string HexString)
			{
				return (IEpcCode) new Epc96(HexString);
			}

			public IEpcCode New(byte[] ByteArray)
			{
				return (IEpcCode) new Epc96(ByteArray);
			}

			public void Clear()
			{
				this.ls64 = 0;
				this.ms32 = 0;
			}

			public byte[] GetBytes()
			{
				UInt64 tempLow = this.ls64;
				UInt32 tempHigh = this.ms32;

				// create a 12-byte array of zero
				byte[] byteArray = new byte[this.ByteLength];

				// shift the integer into the array
				for (int byteNum = byteArray.Length-1; byteNum >= 0; --byteNum)
				{
					byteArray[byteNum] = (byte) (tempLow & 0xff);
					Epc96.ShiftRight8(ref tempLow, ref tempHigh);
				}
				return byteArray;
			}

			public UInt64 Field
			{
				get { return this.ls64; }
				set { this.ls64 = value; }
			}

			public string ToHex()
			{
				UInt64 tempLow = this.ls64;
				UInt32 tempHigh = this.ms32;

				StringBuilder hex = new StringBuilder(this.ByteLength*2);
				for (int x = 0; x < this.ByteLength; ++x)
				{
					hex.Insert(0, (tempLow & 0xff).ToString("X2"));
					Epc96.ShiftRight8(ref tempLow, ref tempHigh);
				}
				return hex.ToString();
			}

			private static void ShiftLeft(ref UInt64 low, ref UInt32 high, int shiftCount)
			{
				while (shiftCount >= 32)
				{
					Epc96.ShiftLeft32(ref low, ref high);
					shiftCount -= 32;
				}
				while (shiftCount >= 16)
				{
					Epc96.ShiftLeft16(ref low, ref high);
					shiftCount -= 16;
				}
				while (shiftCount >= 8)
				{
					Epc96.ShiftLeft8(ref low, ref high);
					shiftCount -= 8;
				}
				Epc96.ShiftLeft1(ref low, ref high, shiftCount);
			}

			private static void ShiftLeft1(ref UInt64 low, ref UInt32 high, int count)
			{
				const UInt64 mask = ((UInt64)1 << (BitLengthLowPart - 1));
				while (count-- > 0)
				{
					high <<= 1;
					if ((low & mask) != 0)
						high |= 1;
					low <<= 1;
				}
			}

			private static void ShiftLeft8(ref UInt64 low, ref UInt32 high)
			{
				high <<= 8;
				UInt64 temp = low >> (BitLengthLowPart - 8);
				high |= (UInt32) (temp & 0xff);
				low <<= 8;
			}

			private static void ShiftLeft16(ref UInt64 low, ref UInt32 high)
			{
				high <<= 16;
				UInt64 temp = low >> (BitLengthLowPart - 16);
				high |= (UInt32) (temp & 0xffff);
				low <<= 16;
			}

			private static void ShiftLeft32(ref UInt64 low, ref UInt32 high)
			{
				high = (UInt32) (low >> 32);
				low <<= 32;
			}

			private static void ShiftRight(ref UInt64 low, ref UInt32 high, int shiftCount)
			{
				while (shiftCount >= 32)
				{
					Epc96.ShiftRight32(ref low, ref high);
					shiftCount -= 32;
				}
				while (shiftCount >= 16)
				{
					Epc96.ShiftRight16(ref low, ref high);
					shiftCount -= 16;
				}
				while (shiftCount >= 8)
				{
					Epc96.ShiftRight8(ref low, ref high);
					shiftCount -= 8;
				}
				Epc96.ShiftRight1(ref low, ref high, shiftCount);
			}
	
			private static void ShiftRight1(ref UInt64 low, ref UInt32 high, int count)
			{
				const UInt64 mask = ((UInt64)1 << (BitLengthLowPart - 1));
				while (count-- > 0)
				{
					low >>= 1;
					if ((high & 1) != 0)
						low |= mask;
					high >>= 1;
				}
			}
			private static void ShiftRight8(ref UInt64 low, ref UInt32 high)
			{
				low >>= 8;
				UInt64 temp = high & 0xff;
				temp <<= BitLengthLowPart - 8;
				low |= temp;
				high >>= 8;
			}
			private static void ShiftRight16(ref UInt64 low, ref UInt32 high)
			{
				low >>= 16;
				UInt64 temp = high & 0xffff;
				temp <<= BitLengthLowPart - 16;
				low |= temp;
				high >>= 16;
			}
			private static void ShiftRight32(ref UInt64 low, ref UInt32 high)
			{
				UInt64 temp = (UInt64) high << 32;
				high = 0;
				low >>= 32;
				low |= temp;
			}
			#region ICloneable Members
			public object Clone()
			{
				return new Epc96(this.ls64, this.ms32);
			}
			#endregion
		}
		#endregion

		#region Codec
		public class Codec : IEpcCodec
		{
			protected internal ArrayList		fieldList;
			protected internal IEpcCode			epcCode;
			protected internal int				headerValue;
			protected internal string			formatName;
			protected internal Epc.Format		encodingFormat;

			private Codec(){}

			/// <summary>
			/// Creates a codec with the specified header value, using the specified EPC data type
			/// </summary>
			/// <param name="HeaderValue">Header value of this format</param>
			/// <param name="RefType">Epc64 or Epc96 data type as appropriate</param>
			public Codec(int HeaderValue, string Name, Epc.Format format, IEpcCode RefType)
			{
				// save the header value
				this.headerValue = HeaderValue;

				// save the format name
				this.formatName = Name;

				// create an empty EPC code
				this.epcCode = RefType.New();

				// save the format
				this.encodingFormat = format;

				// create the field list
				this.fieldList = new ArrayList();
			}

			/// <summary>
			/// Returns the number of bits in the encoded form.
			/// </summary>
			public int BitLength
			{
				get { return this.epcCode.BitLength; }
			}

			/// <summary>
			/// Returns the number of bytes in the encoded form.
			/// </summary>
			public int ByteLength
			{
				get { return this.epcCode.BitLength / 8; }
			}

			/// <summary>
			/// Return the current EPC value.
			/// </summary>
			public IEpcCode EpcCode
			{
				get { return this.epcCode; }
			}

			/// <summary>
			/// Add the definition of a field to this format.
			/// </summary>
			/// <param name="Type"></param>
			/// <returns></returns>
			public Field AddField(FieldID Type)
			{
				Field f = new Field(Type);
				fieldList.Add(f);
				return f;
			}

			/// <summary>
			/// Return number of fields in this format.
			/// </summary>
			public int Count
			{
				get { return this.fieldList.Count; }
			}

			/// <summary>
			/// Returns the string name of the format.
			/// </summary>
			public string Name
			{
				get { return this.formatName; }
			}

			/// <summary>
			/// Returns the encoding format
			/// </summary>
			public Epc.Format Format
			{
				get { return this.encodingFormat; }
			}

			public int Header
			{
				get { return this.headerValue; }
			}

			/// <summary>
			/// Check if the current format requires the named field (type enum)
			/// </summary>
			/// <param name="FieldId"></param>
			/// <returns></returns>
			public bool UsesField(FieldID FieldId)
			{
				for(int x = 0; x < fieldList.Count; ++x)
				{
					if (((Field)fieldList[x]).Type == FieldId)
						return true;
				}
				return false;
			}

			/// <summary>
			/// Returns an array of component field names (type enum)
			/// </summary>
			public FieldID[] ComponentFields
			{
				get
				{
					FieldID[] list = new FieldID[fieldList.Count];
					for(int x = 0; x < fieldList.Count; ++x)
					{
						list[x] = ((Field)fieldList[x]).Type;
					}
					return list;
				}
			}

			/// <summary>
			/// Access a field by its index
			/// </summary>
			public Field this[int index]
			{
				get { return (Field) this.fieldList[index]; }
			}

			/// <summary>
			/// Access the field by field name (type enum)
			/// </summary>
			public Field this[FieldID FieldType]
			{
				get
				{
					for(int x = 0; x < this.fieldList.Count; ++x)
					{
						if (((Field)this.fieldList[x]).Type == FieldType)
							return (Field) this.fieldList[x];
					}
					return null;
				}
			}

			/// <summary>
			/// Return partition value. If all field values have been set, this methid can be used
			/// to determine a partition value that can be used to encode the data. If no partition
			/// value is valid an exception is thrown.
			/// </summary>
			/// <returns>Partiton value 0..n or throws exception if no partition value is valid</returns>
			public int FindPartitionValue()
			{
				// loop through all partition values
				for(int partitionIndex = 0; partitionIndex < 8; ++partitionIndex)
				{
					// assume the partition value is valid
					bool valid = true;

					// loop through all fields to see if this partition will work
					for (int field = 0; valid && field < fieldList.Count; ++field)
					{
						// get the field to check
						Field thisField = (Field) fieldList[field];

						// see if this field uses a partition
						if (thisField.Count > 1)
						{
							// make sure the partition value is valid
							if (partitionIndex >= thisField.Count)
								valid = false;
							else
							{
								// get the partition
								Field.Partition thisPartition = thisField[partitionIndex];

								// make sure the value is within range
								if (thisField.Value > thisPartition.MaxValue)
									valid = false;
							}
						}
					}

					// see if we found a valid value
					if (valid)
						return partitionIndex;
				}
				// no valid partition value was found
				throw new ArgumentException("No valid partition value could be found.");
			}

			/// <summary>
			/// Encode the current format using the field values previously assigned. Returns the IEpcCode result.
			/// </summary>
			/// <returns>IEpcCode result</returns>
			public IEpcCode Encode()
			{
				IEpcCode fieldValue;
				Field.Partition thisPartition;

				// clear the result
				epcCode.Clear();

				// set the header value
				this[FieldID.Header].Value = (UInt64) this.headerValue;

				// get the value of the partition field if there is one
				int partitionNumber = 0;
				if (this.UsesField(FieldID.Partition))
					partitionNumber = (int) this[FieldID.Partition].Value;

				// walk through each field and encode that value
				for(int field = 0; field < this.fieldList.Count; ++field)
				{
					// get the field
					Field thisField = (Field) this.fieldList[field];

					// get the partition we'll use to encode
					if (thisField.Count == 1)
						thisPartition = thisField[0];
					else
					{
						// field is dependent on partition value - make sure partition is valid
						if (partitionNumber < 0 || partitionNumber >= thisField.Count)
							throw new IndexOutOfRangeException(String.Format("Partition value of {0} is invalid; it may not exceed {1}.", partitionNumber, thisField.Count-1));

						// get the partition
						thisPartition = thisField[partitionNumber];
					}

					// see if the field value is within limits
					if (thisField.Type != FieldID.GovtManagedID && thisField.Value > thisPartition.MaxValue)
						throw new ArgumentOutOfRangeException(String.Format("Maximum value is {0}", thisPartition.MaxValue));

					// see if the field length is within limits
					if (thisField.Type != FieldID.GovtManagedID && !thisField.ValidLength(partitionNumber))
						throw new ArgumentOutOfRangeException(String.Format("Input string must be {0} digits", thisPartition.Length));

					// create a new IEpcCode to hold the field
					fieldValue = this.epcCode.New();

					// check for DOD-96 CAGE encoding
					if (this.encodingFormat == Format.DOD96 && thisField.Type == FieldID.GovtManagedID)
					{
						fieldValue.Field = EncodeDOD96CAGE(thisField.StringValue);
					}
					else if (this.encodingFormat == Format.DOD64 && thisField.Type == FieldID.GovtManagedID)
					{
						fieldValue.Field = EncodeDOD64CAGE(thisField.StringValue);
					}
					else
					{
						// just get the binary field
						fieldValue.Field = thisField.Value;
					}

					// shift the field value into position
					fieldValue = fieldValue.ShiftLeft(thisPartition.ShiftCount);

					// make sure the field doesn't overflow to adjacent fieldList
					fieldValue = fieldValue.And(thisPartition.Mask);

					// merge the field into the existing EPC
					epcCode = epcCode.Or(fieldValue);
				}

				return this.epcCode;
			}

			/// <summary>
			/// Encode DOD-96 CAGE field (Government-Managed ID)
			/// The 5-character CAGE code is alphanumeric represented by a space
			/// followed by five characters in the 48-bit field (6 characters of
			/// 8 bits). The string value of the CAGE code must be encoded to a
			/// space followed by 5 ASCII characters encoded into 48 bits and
			/// returned as an integer.
			/// </summary>
			/// <param name="cageField"></param>
			private UInt64 EncodeDOD96CAGE(string cage)
			{
				// take the last five characters
				int len;
				if ((len = cage.Length) < 5)
					throw new ArgumentException("CAGE code must be 5 characters");
				cage = cage.Substring(len - 5).ToUpper();
				UInt64 field = 0x20;
				for (int index = 0; index < cage.Length; ++index)
				{
					// shift the field left 8 bits
					field <<= 8;

					// add the new character
					field |= (UInt64)((byte)cage[index]);
				}
				return field;
			}

			/// <summary>
			/// Encode DOD-64 CAGE field (Government-Managed ID)
			/// The 5-character alphanumeric CAGE code is represented as five
			/// 6-bit characters in the 30-bit CAGE field. Characters A..Z are
			/// mapped to hex values 0x01..0x1A with 0x09 and 0x0F being invalid
			/// (characters 'I' and 'O'). Integer values are still 0x30..0x39.
			/// </summary>
			/// <param name="cage">CAGE code in string form</param>
			/// <returns>Binary value of encoded CAGE code field</returns>
			private UInt64 EncodeDOD64CAGE(string cage)
			{
				// take the last five characters
				int len;
				if ((len = cage.Length) < 5)
					throw new ArgumentException("CAGE code must be 5 characters");
				cage = cage.Substring(len - 5).ToUpper();
				UInt64 field = 0;
				for (int index = 0; index < cage.Length; ++index)
				{
					// shift the field left 6 bits
					field <<= 6;

					// get the next character
					byte ch = (byte)cage[index];

					// see what it is
					if (ch >= 0x41)
					{
						// it's a letter
						ch -= 0x40;	// 'A'-1 so that 'A' becomes 0x01
					}

					// add the new character
					field |= (UInt64)((uint)(ch & 0x3f));
				}
				return field;
			}

			/// <summary>
			/// Decode passed EPC value into its component fields
			/// </summary>
			public void Decode(IEpcCode epcInput)
			{
				IEpcCode fieldValue;
				Field.Partition thisPartition;

				// create an invalid partition value
				int partitionNumber = -1;

				// save the input value
				this.epcCode = epcInput;

				// clear all the output fields
				for(int x = 0; x < this.fieldList.Count; ++x)
					((Field)this.fieldList[x]).Value = 0;

				// walk the field list and extract the fields
				for(int field = 0; field < this.fieldList.Count; ++field)
				{
					Field thisField = (Field) this.fieldList[field];

					// get the partition we'll use to decode - this works because the partition is decoded
					// before any fields that use it are decoded
					if (thisField.Count == 1)
						thisPartition = thisField[0];
					else
					{
						// field is dependent on partition value - make sure partition is valid
						if (partitionNumber < 0 || partitionNumber >= thisField.Count)
							throw new IndexOutOfRangeException(String.Format("Partition value of {0} is invalid; it may not exceed {1}.", partitionNumber, thisField.Count-1));

						// get the partition
						thisPartition = thisField[partitionNumber];
					}

					// copy the current epc value so we don't alter the input value
					fieldValue = (IEpcCode) this.epcCode.Clone();

					// mask off the field
					fieldValue = fieldValue.And(thisPartition.Mask);

					// now shift it into position
					fieldValue = fieldValue.ShiftRight(thisPartition.ShiftCount);

					// check for alphanumeric DOD CAGE code
					if (this.encodingFormat == Format.DOD96 && thisField.Type == FieldID.GovtManagedID)
					{
						thisField.StringValue = DecodeDOD96CAGE(fieldValue);
						thisField.Value = 0;
					}
					else if (this.encodingFormat == Format.DOD64 && thisField.Type == FieldID.GovtManagedID)
					{
						thisField.StringValue = DecodeDOD64CAGE(fieldValue);
						thisField.Value = 0;
					}
					else
					{
						// not DOD CAGE code - save the field string value. This is the correct EPC length
						thisField.StringValue = fieldValue.Field.ToString().PadLeft(thisPartition.Length, '0');

						// save the field integer value - do not do this before saving string above
						thisField.Value = fieldValue.Field;
					}

					// if it's the header, make sure we have the right format
					if (thisField.Type == FieldID.Header)
					{
						if (thisField.Value != (UInt64) this.headerValue)
						{
							throw new InvalidOperationException(String.Format("The current EPC code is not in {0} format. The header value is {1} and it must be {2} for {3}.", this.Name, thisField.Value, this.headerValue, this.Name));
						}
					}

					// otherwise, if it's the partition then save it
					else if (thisField.Type == FieldID.Partition)
						partitionNumber = (int) thisField.Value;
				}
			}

			/// <summary>
			/// Decode DOD-96 CAGE field (Government-Managed ID)
			/// The 5-character CAGE code is alphanumeric represented by a space
			/// followed by five characters in the 48-bit field (6 characters of
			/// 8 bits). The IEpcCode is 96 bits so only the last five bytes are
			/// needed. The CAGE code is converted to a string as there is no
			/// binary representation of a CAGE code.
			/// </summary>
			/// <param name="cageField"></param>
			private string DecodeDOD96CAGE(IEpcCode cageField)
			{
				// break the field into its component bytes (12 bytes)
				byte[] bytes = cageField.GetBytes();

				// the last 5 bytes are ASCII characters
				StringBuilder sb = new StringBuilder();
				for (int index = 7; index < bytes.Length; ++index)
					sb.Append((char)(bytes[index]));
				return sb.ToString();
			}

			/// <summary>
			/// Decode DOD-64 CAGE field (Government-Managed ID)
			/// The 5-character alphanumeric CAGE code is represented as five
			/// 6-bit characters in the 30-bit CAGE field. Characters A..Z are
			/// mapped to hex values 0x01..0x1A with 0x09 and 0x0F being invalid
			/// (characters 'I' and 'O'). Integer values are still 0x30..0x39.
			/// </summary>
			/// <param name="cageField"></param>
			/// <returns></returns>
			private string DecodeDOD64CAGE(IEpcCode cageField)
			{
				// get the raw CAGE field
				UInt32 raw = (UInt32)cageField.Field;
				StringBuilder sb = new StringBuilder();
				for (int index = 0; index < 5; ++index)
				{
					// get the current character from the end of the raw form
					UInt32 code = raw & 0x3f;

					// shift raw value right 6 bits
					raw >>= 6;

					// get the character code
					char ch;
					if (code >= 0x30)
					{
						// it's a number
						ch = (char)code;
					}
					else
					{
						// it's a letter
						ch = (char)(code + (int)('A') - 1);
					}

					// insert the character to get the string in the correct order
					sb.Insert(0, ch.ToString());
				}
				return sb.ToString();
			}

			/// <summary>
			/// Returns the decoded tag in URN form.
			/// </summary>
			public string URN
			{
				get
				{
					StringBuilder sb = new StringBuilder();
					sb.Append("urn:epc:tag:");
					sb.Append(this.Name.ToLower());
					sb.Append(':');

					// append the format-specifiec fields
					switch (this.encodingFormat)
					{
						//urn:epc:tag:usdod-NN:FFF.TTT.SSS
						case Epc.Format.DOD64:
						case Epc.Format.DOD96:
							sb.Append(this[Epc.FieldID.Filter].Value.ToString());
							sb.Append('.');
							sb.Append(this[Epc.FieldID.GovtManagedID].StringValue);
							sb.Append('.');
							sb.Append(this[Epc.FieldID.Serial].Value.ToString());
							break;

						//urn:epc:tag:gid-96:MMM.CCC.SSS
						case Epc.Format.GID96:
							sb.Append(this[Epc.FieldID.GeneralManagerNo].Value.ToString());
							sb.Append('.');
							sb.Append(this[Epc.FieldID.ObjectClass].Value.ToString());
							sb.Append('.');
							sb.Append(this[Epc.FieldID.Serial].Value.ToString());
							break;

						//urn:epc:tag:sgtin-NN:FFF.PPP.III.SSS
						case Epc.Format.SGTIN64:
						case Epc.Format.SGTIN96:
							sb.Append(this[Epc.FieldID.Filter].Value.ToString());
							sb.Append('.');
							sb.Append(this[Epc.FieldID.Company].Value.ToString());
							sb.Append('.');
							sb.Append(this[Epc.FieldID.Item].Value.ToString());
							sb.Append('.');
							sb.Append(this[Epc.FieldID.Serial].Value.ToString());
							break;

						//urn:epc:tag:sscc-NN:FFF.PPP.III
						case Epc.Format.SSCC64:
						case Epc.Format.SSCC96:
							sb.Append(this[Epc.FieldID.Filter].Value.ToString());
							sb.Append('.');
							sb.Append(this[Epc.FieldID.Company].Value.ToString());
							sb.Append('.');
							sb.Append(this[Epc.FieldID.Serial].Value.ToString());
							break;

						//urn:epc:tag:sgln-NN:FFF.PPP.LLL.SSS
						case Epc.Format.SGLN64:
						case Epc.Format.SGLN96:
							sb.Append(this[Epc.FieldID.Filter].Value.ToString());
							sb.Append('.');
							sb.Append(this[Epc.FieldID.Company].Value.ToString());
							sb.Append('.');
							sb.Append(this[Epc.FieldID.Location].Value.ToString());
							sb.Append('.');
							sb.Append(this[Epc.FieldID.Serial].Value.ToString());
							break;

						//urn:epc:tag:grai-NN:FFF.PPP.III.SSS
						case Epc.Format.GRAI64:
						case Epc.Format.GRAI96:
							sb.Append(this[Epc.FieldID.Filter].Value.ToString());
							sb.Append('.');
							sb.Append(this[Epc.FieldID.Company].Value.ToString());
							sb.Append('.');
							sb.Append(this[Epc.FieldID.Asset].Value.ToString());
							sb.Append('.');
							sb.Append(this[Epc.FieldID.Serial].Value.ToString());
							break;

						//urn:epc:tag:giai-NN:FFF.PPP.SSS
						case Epc.Format.GIAI64:
						case Epc.Format.GIAI96:
							sb.Append(this[Epc.FieldID.Filter].Value.ToString());
							sb.Append('.');
							sb.Append(this[Epc.FieldID.Company].Value.ToString());
							sb.Append('.');
							sb.Append(this[Epc.FieldID.Asset].Value.ToString());
							break;
					}

					return sb.ToString();
				}
			}

			// an EPC 'field' is composed of the field identifier, the value, and its partition table. The
			// partition table will contain only one entry if the field is independent of the partition value.
			public class Field
			{
				private FieldID			type;					// field type
				private	UInt64			integerValue;			// current integer value
				private string			stringValue;			// current string value
				private bool			checkLength;			// true if field value given as string
				private ArrayList		partitionTable;			// partition table

				private Field(){}

				/// <summary>
				/// Creates a field of the specified type (name)
				/// </summary>
				/// <param name="FieldType"></param>
				public Field(FieldID FieldType)
				{
					this.type = FieldType;
					this.integerValue = 0;
					this.stringValue = "";
					this.checkLength = false;
					this.partitionTable = new ArrayList();
				}

				/// <summary>
				/// Adds a partition to the field.
				/// </summary>
				/// <param name="Partition">Partition entry to add</param>
				public void AddPartition(Partition Partition)
				{
					this.partitionTable.Add(Partition);
				}

				/// <summary>
				/// Returns the name (type enum) for this field.
				/// </summary>
				public FieldID Type
				{
					get { return this.type; }
				}

				/// <summary>
				/// Sets or gets the 64-bit field value.
				/// </summary>
				public UInt64 Value
				{
					get
					{
						// throw exception if this is a DOD-64 or DOD-96 CAGE code
						// which can only be represented as a string
						if (this.type == FieldID.GovtManagedID)
							throw new ArgumentException("CAGE code cannot be represented as an integer");
						return this.integerValue;
					}
					set
					{
						if (this.type == FieldID.GovtManagedID && value != 0)
							throw new ArgumentException("CAGE code cannot be represented as an integer");
						this.integerValue = value;
						this.checkLength = false;
					}
				}

				public bool ValidLength(int partitionNumber)
				{
					if (!this.checkLength)
						return true;

					return (this.stringValue.Length == ((Partition)this.partitionTable[partitionNumber]).Length);
				}

				/// <summary>
				/// Sets of gets the field value as a string
				/// </summary>
				public string StringValue
				{
					get
					{
						return this.stringValue;
					}
					set
					{
						// CAGE code cannot be represented as an integer
						if (this.type == FieldID.GovtManagedID)
						{
							if (value.Length != 5)
								throw new ArgumentException("Invalid CAGE code length.");
							foreach (char ch in value)
							{
								if (ch == 'I' || ch == 'O')
									throw new ArgumentException("CAGE code may not contain \'I\' or \'O\'.");
							}
							this.stringValue = value;
							this.integerValue = 0;
							this.checkLength = false;
							return;
						}

						// make sure the length matches at least one of the
						// partitions
						foreach (Partition p in this.partitionTable)
						{
							if (value.Length == p.Length)
							{
								// save the value
								this.stringValue = value;
								this.integerValue = Convert.ToUInt64(value);
								this.checkLength = true;
								return;
							}
						}
						// didn't match any partition
						throw new ArgumentException("Invalid field length.");
					}
				}

				/// <summary>
				/// Gets and sets the partitions of this field by index.
				/// </summary>
				public Partition this[int partition]
				{
					get { return (Partition) this.partitionTable[partition]; }
					set { this.partitionTable[partition] = value; }
				}

				/// <summary>
				/// Returns the number of partitions in this field.
				/// </summary>
				public int Count
				{
					get { return partitionTable.Count; }
				}

				/// <summary>
				/// An EPC 'partition' contains all the information for inserting and extracting the field 
				/// </summary>
				public class Partition
				{
					private	IEpcCode	mask;
					private int			shiftCount;
					private UInt64		maxValue;
					private int			length;

					/// <summary>
					/// Create a partition given the mask, shift count, maximum value and maximum digit count.
					/// </summary>
					/// <param name="FieldMask"></param>
					/// <param name="ShiftCount"></param>
					/// <param name="MaxValue"></param>
					/// <param name="MaxDigits"></param>
					public Partition(IEpcCode FieldMask, int ShiftCount, UInt64 MaxValue, int MaxDigits)
					{
						this.mask = FieldMask;
						this.shiftCount = ShiftCount;
						this.maxValue = MaxValue;
						this.length = MaxDigits;
					}

					/// <summary>
					/// Mask value that will isolate the field in the EPC code.
					/// </summary>
					public IEpcCode Mask
					{
						get { return this.mask; }
					}

					/// <summary>
					/// Number of left shifts required to align the field in the EPC code.
					/// </summary>
					public int ShiftCount
					{
						get { return this.shiftCount; }
					}

					/// <summary>
					/// Maximum value the field can hold for this partition value.
					/// </summary>
					public UInt64 MaxValue
					{
						get { return this.maxValue; }
					}

					/// <summary>
					/// Maximum number of digits that can be represented for this partition value.
					/// For strict EPC encoding the input string must be this length. On decode
					/// the number must be this length.
					/// </summary>
					public int Length
					{
						get { return this.length; }
					}

					/// <summary>
					/// Build a partition given the starting bit and the bit length
					/// </summary>
					/// <param name="StartBit"></param>
					/// <param name="BitLength"></param>
					/// <returns></returns>
					public static Partition Build(IEpcCode RefType, int StartBit, int BitLength, UInt64 maxValue, int maxDigits)
					{
						// create a blank EPC code
						IEpcCode mask = RefType.New();

						// create a mask value of 1
						IEpcCode one = RefType.New();
						one.Field = 1;

						// calculate the ending bit
						int endBit = StartBit + BitLength;

						// generate the mask
						for(int bit = 0; bit < RefType.BitLength; ++bit)
						{
							mask = mask.ShiftLeft(1);
							if (bit >= StartBit && bit < endBit)
							{
								mask = mask.Or(one);
							}
						}

						return new Partition(mask, RefType.BitLength - (StartBit + BitLength), maxValue, maxDigits);
					}
				}
			}
		}
		#endregion

		/// <summary>
		/// Represents the GID-96 format.
		/// </summary>
		public class gid96 : Epc.Codec
		{
			public const int HeaderValue = 0x35;
			public const Epc.Format EncodingFormat = Epc.Format.GID96;

			public gid96() : base(HeaderValue, "GID-96", EncodingFormat, new Epc96())
			{
				Field field;
				int startBit = 0;
				int length;
				IEpcCode RefType = new Epc96();

				// define the header field and set its value
				field = base.AddField(FieldID.Header);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 8, HeaderValue, 2));
				startBit += length;

				// define the General Manager Number field
				field = base.AddField(FieldID.GeneralManagerNo);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 28, 268435455, 9));
				startBit += length;

				// define the Object Class field
				field = base.AddField(FieldID.ObjectClass);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 24, 16777215, 8));
				startBit += length;

				// define the serial number field
				field = base.AddField(FieldID.Serial);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 36, 68719476735, 11));
				startBit += length;
			}

			static public bool MatchesHeader(byte headerByte)
			{
				return headerByte == (byte) HeaderValue;
			}
		}

		/// <summary>
		/// Represents the SGTIN-64 format.
		/// </summary>
		public class sgtin64 : Epc.Codec
		{
			public const int HeaderValue = 0x02;
			public const Epc.Format EncodingFormat = Epc.Format.SGTIN64;

			public sgtin64() : base(HeaderValue, "SGTIN-64", EncodingFormat, new Epc64())
			{
				Field field;
				int startBit = 0;
				int length;
				IEpcCode RefType = new Epc64();

				// define the header field and set its value
				field = base.AddField(FieldID.Header);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 2, HeaderValue, 1));
				startBit += length;

				// define the filter field
				field = base.AddField(FieldID.Filter);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 3, 8, 1));
				startBit += length;

				// define the Company field
				field = base.AddField(FieldID.Company);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 14, 16383, 5));
				startBit += length;

				// define the item reference field
				field = base.AddField(FieldID.Item);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 20, 1048575, 7));
				startBit += length;

				// define the serial number field
				field = base.AddField(FieldID.Serial);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 25, 33554431, 8));
				startBit += length;
			}

			static public bool MatchesHeader(byte headerByte)
			{
				return (headerByte & 0xc0) == (byte) (HeaderValue << 6);
			}
		}

		/// <summary>
		/// Represents the SGTIN-96 format.
		/// </summary>
		public class sgtin96 : Epc.Codec
		{
			public const int HeaderValue = 0x30;
			public const Epc.Format EncodingFormat = Epc.Format.SGTIN96;

			public sgtin96() : base(HeaderValue, "SGTIN-96", EncodingFormat, new Epc96())
			{
				Field field;
				int startBit = 0;
				int length;
				IEpcCode RefType = new Epc96();

				// define the header field and set its value
				field = base.AddField(FieldID.Header);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 8, HeaderValue, 2));
				startBit += length;

				// define the filter field
				field = base.AddField(FieldID.Filter);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 3, 8, 1));
				startBit += length;

				// define the partition field
				field = base.AddField(FieldID.Partition);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 3, 6, 1));
				startBit += length;

				// define the Company field
				field = base.AddField(FieldID.Company);

				// populate the partition table for the Company field
				field.AddPartition(Field.Partition.Build(RefType, startBit, 40, 999999999999, 12));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 37,  99999999999, 11));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 34,   9999999999, 10));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 30,    999999999, 9));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 27,     99999999, 8));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 24,      9999999, 7));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 20,       999999, 6));

				// define the item reference field
				field = base.AddField(FieldID.Item);

				// populate the partition table for the Item field
				field.AddPartition(Field.Partition.Build(RefType, startBit + 40,  4,       9, 1));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 37,  7,      99, 2));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 34, 10,     999, 3));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 30, 14,    9999, 4));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 27, 17,   99999, 5));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 24, 20,  999999, 6));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 20, 24, 9999999, 7));
				startBit += 44;

				// define the serial number field
				field = base.AddField(FieldID.Serial);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 38, (UInt64) 274877906943, 12));
				startBit += length;
			}

			static public bool MatchesHeader(byte headerByte)
			{
				return headerByte == (byte) HeaderValue;
			}
		}

		/// <summary>
		/// Represents the SSCC-64 format.
		/// </summary>
		public class sscc64 : Epc.Codec
		{
			public const int HeaderValue = 0x08;
			public const Epc.Format EncodingFormat = Epc.Format.SSCC64;

			public sscc64() : base(HeaderValue, "SSCC-64", EncodingFormat, new Epc64())
			{
				Field field;
				int startBit = 0;
				int length;
				IEpcCode RefType = new Epc64();

				// define the header field and set its value
				field = base.AddField(FieldID.Header);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 8, HeaderValue, 2));
				startBit += length;

				// define the filter field
				field = base.AddField(FieldID.Filter);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 3, 7, 1));
				startBit += length;

				// define the Company field
				field = base.AddField(FieldID.Company);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 14, 16383, 5));
				startBit += length;

				// define the serial number field
				field = base.AddField(FieldID.Serial);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 39, 99999999999, 11));
				startBit += length;
			}

			static public bool MatchesHeader(byte headerByte)
			{
				return headerByte == (byte) HeaderValue;
			}
		}
	
		/// <summary>
		/// Represents the SSCC-96 format.
		/// </summary>
		public class sscc96 : Epc.Codec
		{
			public const int HeaderValue = 0x31;
			public const Epc.Format EncodingFormat = Epc.Format.SSCC96;

			public sscc96() : base(HeaderValue, "SSCC-96", EncodingFormat, new Epc96())
			{
				Field field;
				int startBit = 0;
				int length;
				IEpcCode RefType = new Epc96();

				// define the header field and set its value
				field = base.AddField(FieldID.Header);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 8, HeaderValue, 2));
				startBit += length;

				// define the filter field
				field = base.AddField(FieldID.Filter);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 3, 7, 1));
				startBit += length;

				// define the partition field
				field = base.AddField(FieldID.Partition);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 3, 6, 1));
				startBit += length;

				// define the Company field
				field = base.AddField(FieldID.Company);

				// populate the partition table for the Company field
				field.AddPartition(Field.Partition.Build(RefType, startBit, 40, 999999999999, 12));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 37,  99999999999, 11));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 34,   9999999999, 10));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 30,    999999999,  9));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 27,     99999999,  8));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 24,      9999999,  7));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 20,       999999,  6));

				// define the serial reference field
				field = base.AddField(FieldID.Serial);

				// populate the partition table for the Serial field
				field.AddPartition(Field.Partition.Build(RefType, startBit + 40, 18,       99999,  5));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 37, 21,      999999,  6));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 34, 24,     9999999,  7));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 30, 28,    99999999,  8));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 27, 31,   999999999,  9));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 24, 34,  9999999999, 10));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 20, 38, 99999999999, 11));
				startBit += length;
			}

			static public bool MatchesHeader(byte headerByte)
			{
				return headerByte == (byte) HeaderValue;
			}
		}

		/// <summary>
		/// Represents the SGLN-64 format.
		/// </summary>
		public class sgln64 : Epc.Codec
		{
			public const int HeaderValue = 0x09;
			public const Epc.Format EncodingFormat = Epc.Format.SGLN64;

			public sgln64() : base(HeaderValue, "SGLN-64", EncodingFormat, new Epc64())
			{
				Field field;
				int startBit = 0;
				int length;
				IEpcCode RefType = new Epc64();

				// define the header field and set its value
				field = base.AddField(FieldID.Header);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 8, HeaderValue, 2));
				startBit += length;

				// define the filter field
				field = base.AddField(FieldID.Filter);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 3, 7, 1));
				startBit += length;

				// define the Company field
				field = base.AddField(FieldID.Company);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 14, 16383, 5));
				startBit += length;

				// define the Location field
				field = base.AddField(FieldID.Location);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 20, 999999, 6));
				startBit += length;

				// define the serial number field
				field = base.AddField(FieldID.Serial);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 19, 524287, 6));
				startBit += length;
			}

			static public bool MatchesHeader(byte headerByte)
			{
				return headerByte == (byte) HeaderValue;
			}
		}

		/// <summary>
		/// Represents the SGLN-96 format.
		/// </summary>
		public class sgln96 : Epc.Codec
		{
			public const int HeaderValue = 0x32;
			public const Epc.Format EncodingFormat = Epc.Format.SGLN96;

			public sgln96() : base(HeaderValue, "SGLN-96", EncodingFormat, new Epc96())
			{
				Field field;
				int startBit = 0;
				int length;
				IEpcCode RefType = new Epc96();

				// define the header field and set its value
				field = base.AddField(FieldID.Header);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 8, HeaderValue, 2));
				startBit += length;

				// define the filter field
				field = base.AddField(FieldID.Filter);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 3, 7, 1));
				startBit += length;

				// define the partition field
				field = base.AddField(FieldID.Partition);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 3, 6, 1));
				startBit += length;

				// define the Company field
				field = base.AddField(FieldID.Company);

				// populate the partition table for the Company field
				field.AddPartition(Field.Partition.Build(RefType, startBit, 40, 999999999999, 12));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 37,  99999999999, 11));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 34,   9999999999, 10));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 30,    999999999, 9));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 27,     99999999, 8));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 24,      9999999, 7));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 20,       999999, 6));

				// define the Location reference field
				field = base.AddField(FieldID.Location);

				// populate the partition table for the Item field
				field.AddPartition(Field.Partition.Build(RefType, startBit + 40,  1,      1, 0));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 37,  4,      9, 1));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 34,  7,     99, 2));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 30, 11,    999, 3));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 27, 14,   9999, 4));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 24, 17,  99999, 5));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 20, 21, 999999, 6));
				startBit += 41;

				// define the serial number field
				field = base.AddField(FieldID.Serial);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 41, 2199023255552, 13));
				startBit += length;
			}

			static public bool MatchesHeader(byte headerByte)
			{
				return headerByte == (byte) HeaderValue;
			}
		}

		/// <summary>
		/// Represents the GRAI-64 format.
		/// </summary>
		public class grai64 : Epc.Codec
		{
			public const int HeaderValue = 0x0a;
			public const Epc.Format EncodingFormat = Epc.Format.GRAI64;

			public grai64() : base(HeaderValue, "GRAI-64", EncodingFormat, new Epc64())
			{
				Field field;
				int startBit = 0;
				int length;
				IEpcCode RefType = new Epc64();

				// define the header field and set its value
				field = base.AddField(FieldID.Header);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 8, HeaderValue, 2));
				startBit += length;

				// define the filter field
				field = base.AddField(FieldID.Filter);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 3, 8, 1));
				startBit += length;

				// define the Company field
				field = base.AddField(FieldID.Company);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 14, 16383, 5));
				startBit += length;

				// define the Asset reference field
				field = base.AddField(FieldID.Asset);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 20, 1048575, 7));
				startBit += length;

				// define the serial number field
				field = base.AddField(FieldID.Serial);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 19, 524287, 6));
				startBit += length;
			}

			static public bool MatchesHeader(byte headerByte)
			{
				return headerByte == (byte) HeaderValue;
			}
		}

		/// <summary>
		/// Represents the GRAI-96 format.
		/// </summary>
		public class grai96 : Epc.Codec
		{
			public const int HeaderValue = 0x33;
			public const Epc.Format EncodingFormat = Epc.Format.GRAI96;

			public grai96() : base(HeaderValue, "GRAI-96", EncodingFormat, new Epc96())
			{
				Field field;
				int startBit = 0;
				int length;
				IEpcCode RefType = new Epc96();

				// define the header field and set its value
				field = base.AddField(FieldID.Header);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 8, HeaderValue, 2));
				startBit += length;

				// define the filter field
				field = base.AddField(FieldID.Filter);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 3, 7, 1));
				startBit += length;

				// define the partition field
				field = base.AddField(FieldID.Partition);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 3, 6, 1));
				startBit += length;

				// define the Company field
				field = base.AddField(FieldID.Company);

				// populate the partition table for the Company field
				field.AddPartition(Field.Partition.Build(RefType, startBit, 40, 999999999999, 12));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 37,  99999999999, 11));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 34,   9999999999, 10));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 30,    999999999, 9));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 27,     99999999, 8));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 24,      9999999, 7));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 20,       999999, 6));

				// define the asset reference field
				field = base.AddField(FieldID.Asset);

				// populate the partition table for the asset field
				field.AddPartition(Field.Partition.Build(RefType, startBit + 40,  4,       9, 1));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 37,  7,      99, 2));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 34, 10,     999, 3));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 30, 14,    9999, 4));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 27, 17,   99999, 5));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 24, 20,  999999, 6));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 20, 24, 9999999, 7));
				startBit += 44;

				// define the serial number field
				field = base.AddField(FieldID.Serial);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 38, (UInt64) 274877906943, 12));
				startBit += length;
			}

			static public bool MatchesHeader(byte headerByte)
			{
				return headerByte == (byte) HeaderValue;
			}
		}

		/// <summary>
		/// Represents the GIAI-64 format.
		/// </summary>
		public class giai64 : Epc.Codec
		{
			public const int HeaderValue = 0x0b;
			public const Epc.Format EncodingFormat = Epc.Format.GIAI64;

			public giai64() : base(HeaderValue, "GIAI-64", EncodingFormat, new Epc64())
			{
				Field field;
				int startBit = 0;
				int length;
				IEpcCode RefType = new Epc64();

				// define the header field and set its value
				field = base.AddField(FieldID.Header);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 8, HeaderValue, 1));
				startBit += length;

				// define the filter field
				field = base.AddField(FieldID.Filter);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 3, 7, 1));
				startBit += length;

				// define the Company field
				field = base.AddField(FieldID.Company);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 14, 16383, 5));
				startBit += length;

				// define the Asset Reference field
				field = base.AddField(FieldID.Asset);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 39, 549755813888, 12));
				startBit += length;
			}

			static public bool MatchesHeader(byte headerByte)
			{
				return headerByte == (byte) HeaderValue;
			}
		}

		/// <summary>
		/// Represents the GIAI-96 format.
		/// </summary>
		public class giai96 : Epc.Codec
		{
			public const int HeaderValue = 0x34;
			public const Epc.Format EncodingFormat = Epc.Format.GIAI96;

			public giai96() : base(HeaderValue, "GIAI-96", EncodingFormat, new Epc96())
			{
				Field field;
				int startBit = 0;
				int length;
				IEpcCode RefType = new Epc96();

				// define the header field and set its value
				field = base.AddField(FieldID.Header);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 8, HeaderValue, 2));
				startBit += length;

				// define the filter field
				field = base.AddField(FieldID.Filter);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 3, 7, 1));
				startBit += length;

				// define the partition field
				field = base.AddField(FieldID.Partition);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 3, 6, 1));
				startBit += length;

				// define the Company field
				field = base.AddField(FieldID.Company);

				// populate the partition table for the Company field
				field.AddPartition(Field.Partition.Build(RefType, startBit, 40, 999999999999, 12));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 37,  99999999999, 11));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 34,   9999999999, 10));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 30,    999999999, 9));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 27,     99999999, 8));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 24,      9999999, 7));
				field.AddPartition(Field.Partition.Build(RefType, startBit, 20,       999999, 6));

				// define the asset reference field
				field = base.AddField(FieldID.Asset);

				// populate the partition table for the asset field
				field.AddPartition(Field.Partition.Build(RefType, startBit + 40, 42,       4398046511104, 12));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 37, 45,      35184372088832, 13));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 34, 48,     281474976710656, 14));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 30, 52,    4503599627370496, 15));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 27, 55,   36028797018963968, 16));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 24, 58,  288230376151711744, 17));
				field.AddPartition(Field.Partition.Build(RefType, startBit + 20, 62, 4611686018427387904, 18));
				startBit += 82;
			}

			static public bool MatchesHeader(byte headerByte)
			{
				return headerByte == (byte)HeaderValue;
			}
		}

		/// <summary>
		/// Represents the DOD-64 format.
		/// </summary>
		public class dod64 : Epc.Codec
		{
			public const int HeaderValue = 0xce;
			public const Epc.Format EncodingFormat = Epc.Format.DOD64;

			public dod64() : base(HeaderValue, "USDOD-64", EncodingFormat, new Epc64())
			{
				Field field;
				int startBit = 0;
				int length;
				IEpcCode RefType = new Epc64();

				// define the header field and set its value
				field = base.AddField(FieldID.Header);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 8, HeaderValue, 1));
				startBit += length;

				// define the filter field
				field = base.AddField(FieldID.Filter);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 2, 3, 1));
				startBit += length;

				// define the government-managed identifier (CAGE code)
				field = base.AddField(FieldID.GovtManagedID);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 30, 999999999, 9));
				startBit += length;

				// define the serial number field
				field = base.AddField(FieldID.Serial);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 24, 16777215, 8));
				startBit += length;
			}

			static public bool MatchesHeader(byte headerByte)
			{
				return headerByte == (byte) HeaderValue;
			}
		}

		/// <summary>
		/// Represents the DOD-96 format.
		/// </summary>
		public class dod96 : Epc.Codec
		{
			public const int HeaderValue = 0x2f;
			public const Epc.Format EncodingFormat = Epc.Format.DOD96;

			public dod96() : base(HeaderValue, "USDOD-96", EncodingFormat, new Epc96())
			{
				Field field;
				int startBit = 0;
				int length;
				IEpcCode RefType = new Epc96();

				// define the header field and set its value
				field = base.AddField(FieldID.Header);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 8, HeaderValue, 2));
				startBit += length;

				// define the filter field
				field = base.AddField(FieldID.Filter);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 4, 15, 2));
				startBit += length;

				// define the government-managed identifier (CAGE code)
				field = base.AddField(FieldID.GovtManagedID);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 48, 281474976710656, 14));
				startBit += length;

				// define the serial number field
				field = base.AddField(FieldID.Serial);
				field.AddPartition(Field.Partition.Build(RefType, startBit, length = 36, 68719476735, 11));
				startBit += length;
			}

			static public bool MatchesHeader(byte headerByte)
			{
				return headerByte == (byte) HeaderValue;
			}
		}
	}
}
