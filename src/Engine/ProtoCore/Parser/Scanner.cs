
using System;
using System.Collections;
using System.IO;

namespace ProtoCore.DesignScriptParser
{

    public class Token {
	public int kind;    // token kind
	public int pos;     // token position in bytes in the source text (starting at 0)
	public int charPos;  // token position in characters in the source text (starting at 0)
	public int col;     // token column (starting at 1)
	public int line;    // token line (starting at 1)
	public string val;  // token value
	public Token next;  // ML 2005-03-11 Tokens are kept in linked list
}

//-----------------------------------------------------------------------------------
// Buffer
//-----------------------------------------------------------------------------------
public class Buffer {
	// This Buffer supports the following cases:
	// 1) seekable stream (file)
	//    a) whole stream in buffer
	//    b) part of stream in buffer
	// 2) non seekable stream (network, console)

	public const int EOF = char.MaxValue + 1;
	const int MIN_BUFFER_LENGTH = 1024; // 1KB
	const int MAX_BUFFER_LENGTH = MIN_BUFFER_LENGTH * 64; // 64KB
	byte[] buf;         // input buffer
	int bufStart;       // position of first byte in buffer relative to input stream
	int bufLen;         // length of buffer
	int fileLen;        // length of input stream (may change if the stream is no file)
	int bufPos;         // current position in buffer
	Stream stream;      // input stream (seekable)
	readonly bool isUserStream;  // was the stream opened by the user?
	
	public Buffer (Stream s, bool isUserStream) {
		stream = s; this.isUserStream = isUserStream;
		
		if (stream.CanSeek) {
			fileLen = (int) stream.Length;
			bufLen = Math.Min(fileLen, MAX_BUFFER_LENGTH);
			bufStart = Int32.MaxValue; // nothing in the buffer so far
		} else {
			fileLen = bufLen = bufStart = 0;
		}

		buf = new byte[(bufLen>0) ? bufLen : MIN_BUFFER_LENGTH];
		if (fileLen > 0) Pos = 0; // setup buffer to position 0 (start)
		else bufPos = 0; // index 0 is already after the file, thus Pos = 0 is invalid
		if (bufLen == fileLen && stream.CanSeek) Close();
	}
	
	protected Buffer(Buffer b) { // called in UTF8Buffer constructor
		buf = b.buf;
		bufStart = b.bufStart;
		bufLen = b.bufLen;
		fileLen = b.fileLen;
		bufPos = b.bufPos;
		stream = b.stream;
		// keep destructor from closing the stream
		b.stream = null;
		isUserStream = b.isUserStream;
	}

	~Buffer() { Close(); }
	
	protected void Close() {
		if (!isUserStream && stream != null) {
			stream.Close();
			stream = null;
		}
	}
	
	public virtual int Read () {
		if (bufPos < bufLen) {
			return buf[bufPos++];
		} else if (Pos < fileLen) {
			Pos = Pos; // shift buffer start to Pos
			return buf[bufPos++];
		} else if (stream != null && !stream.CanSeek && ReadNextStreamChunk() > 0) {
			return buf[bufPos++];
		} else {
			return EOF;
		}
	}

	public int Peek () {
		int curPos = Pos;
		int ch = Read();
		Pos = curPos;
		return ch;
	}
	
	// beg .. begin, zero-based, inclusive, in byte
	// end .. end, zero-based, exclusive, in byte
	public string GetString (int beg, int end) {
		int len = 0;
		char[] buf = new char[end - beg];
		int oldPos = Pos;
		Pos = beg;
		while (Pos < end) buf[len++] = (char) Read();
		Pos = oldPos;
		return new String(buf, 0, len);
	}

	public int Pos {
		get { return bufPos + bufStart; }
		set {
			if (value >= fileLen && stream != null && !stream.CanSeek) {
				// Wanted position is after buffer and the stream
				// is not seek-able e.g. network or console,
				// thus we have to read the stream manually till
				// the wanted position is in sight.
				while (value >= fileLen && ReadNextStreamChunk() > 0);
			}

			if (value < 0 || value > fileLen) {
				throw new FatalError("buffer out of bounds access, position: " + value);
			}

			if (value >= bufStart && value < bufStart + bufLen) { // already in buffer
				bufPos = value - bufStart;
			} else if (stream != null) { // must be swapped in
				stream.Seek(value, SeekOrigin.Begin);
				bufLen = stream.Read(buf, 0, buf.Length);
				bufStart = value; bufPos = 0;
			} else {
				// set the position to the end of the file, Pos will return fileLen.
				bufPos = fileLen - bufStart;
			}
		}
	}
	
	// Read the next chunk of bytes from the stream, increases the buffer
	// if needed and updates the fields fileLen and bufLen.
	// Returns the number of bytes read.
	private int ReadNextStreamChunk() {
		int free = buf.Length - bufLen;
		if (free == 0) {
			// in the case of a growing input stream
			// we can neither seek in the stream, nor can we
			// foresee the maximum length, thus we must adapt
			// the buffer size on demand.
			byte[] newBuf = new byte[bufLen * 2];
			Array.Copy(buf, newBuf, bufLen);
			buf = newBuf;
			free = bufLen;
		}
		int read = stream.Read(buf, bufLen, free);
		if (read > 0) {
			fileLen = bufLen = (bufLen + read);
			return read;
		}
		// end of stream reached
		return 0;
	}
}

//-----------------------------------------------------------------------------------
// UTF8Buffer
//-----------------------------------------------------------------------------------
public class UTF8Buffer: Buffer {
	public UTF8Buffer(Buffer b): base(b) {}

	public override int Read() {
		int ch;
		do {
			ch = base.Read();
			// until we find a utf8 start (0xxxxxxx or 11xxxxxx)
		} while ((ch >= 128) && ((ch & 0xC0) != 0xC0) && (ch != EOF));
		if (ch < 128 || ch == EOF) {
			// nothing to do, first 127 chars are the same in ascii and utf8
			// 0xxxxxxx or end of file character
		} else if ((ch & 0xF0) == 0xF0) {
			// 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
			int c1 = ch & 0x07; ch = base.Read();
			int c2 = ch & 0x3F; ch = base.Read();
			int c3 = ch & 0x3F; ch = base.Read();
			int c4 = ch & 0x3F;
			ch = (((((c1 << 6) | c2) << 6) | c3) << 6) | c4;
		} else if ((ch & 0xE0) == 0xE0) {
			// 1110xxxx 10xxxxxx 10xxxxxx
			int c1 = ch & 0x0F; ch = base.Read();
			int c2 = ch & 0x3F; ch = base.Read();
			int c3 = ch & 0x3F;
			ch = (((c1 << 6) | c2) << 6) | c3;
		} else if ((ch & 0xC0) == 0xC0) {
			// 110xxxxx 10xxxxxx
			int c1 = ch & 0x1F; ch = base.Read();
			int c2 = ch & 0x3F;
			ch = (c1 << 6) | c2;
		}
		return ch;
	}
}

//-----------------------------------------------------------------------------------
// Scanner
//-----------------------------------------------------------------------------------
public class Scanner {
	const char EOL = '\n';
	const int eofSym = 0; /* pdt */
	const int maxT = 65;
	const int noSym = 65;


	public Buffer buffer; // scanner buffer
	
	Token t;          // current token
	int ch;           // current input character
	int pos;          // byte position of current character
	int charPos;      // position by unicode characters starting with 0
	int col;          // column number of current character
	int line;         // line number of current character
	int oldEols;      // EOLs that appeared in a comment;
	static readonly Hashtable start; // maps first token character to start state

	Token tokens;     // list of tokens already peeked (first token is a dummy)
	Token pt;         // current peek token
	
	char[] tval = new char[128]; // text of current token
	int tlen;         // length of current token
	
	static Scanner() {
		start = new Hashtable(128);
		for (int i = 65; i <= 90; ++i) start[i] = 1;
		for (int i = 95; i <= 95; ++i) start[i] = 1;
		for (int i = 97; i <= 122; ++i) start[i] = 1;
		for (int i = 170; i <= 170; ++i) start[i] = 1;
		for (int i = 181; i <= 181; ++i) start[i] = 1;
		for (int i = 186; i <= 186; ++i) start[i] = 1;
		for (int i = 192; i <= 214; ++i) start[i] = 1;
		for (int i = 216; i <= 246; ++i) start[i] = 1;
		for (int i = 248; i <= 705; ++i) start[i] = 1;
		for (int i = 710; i <= 721; ++i) start[i] = 1;
		for (int i = 736; i <= 740; ++i) start[i] = 1;
		for (int i = 748; i <= 748; ++i) start[i] = 1;
		for (int i = 750; i <= 750; ++i) start[i] = 1;
		for (int i = 880; i <= 884; ++i) start[i] = 1;
		for (int i = 886; i <= 887; ++i) start[i] = 1;
		for (int i = 890; i <= 893; ++i) start[i] = 1;
		for (int i = 902; i <= 902; ++i) start[i] = 1;
		for (int i = 904; i <= 906; ++i) start[i] = 1;
		for (int i = 908; i <= 908; ++i) start[i] = 1;
		for (int i = 910; i <= 929; ++i) start[i] = 1;
		for (int i = 931; i <= 1013; ++i) start[i] = 1;
		for (int i = 1015; i <= 1153; ++i) start[i] = 1;
		for (int i = 1162; i <= 1319; ++i) start[i] = 1;
		for (int i = 1329; i <= 1366; ++i) start[i] = 1;
		for (int i = 1369; i <= 1369; ++i) start[i] = 1;
		for (int i = 1377; i <= 1415; ++i) start[i] = 1;
		for (int i = 1488; i <= 1514; ++i) start[i] = 1;
		for (int i = 1520; i <= 1522; ++i) start[i] = 1;
		for (int i = 1568; i <= 1610; ++i) start[i] = 1;
		for (int i = 1646; i <= 1647; ++i) start[i] = 1;
		for (int i = 1649; i <= 1747; ++i) start[i] = 1;
		for (int i = 1749; i <= 1749; ++i) start[i] = 1;
		for (int i = 1765; i <= 1766; ++i) start[i] = 1;
		for (int i = 1774; i <= 1775; ++i) start[i] = 1;
		for (int i = 1786; i <= 1788; ++i) start[i] = 1;
		for (int i = 1791; i <= 1791; ++i) start[i] = 1;
		for (int i = 1808; i <= 1808; ++i) start[i] = 1;
		for (int i = 1810; i <= 1839; ++i) start[i] = 1;
		for (int i = 1869; i <= 1957; ++i) start[i] = 1;
		for (int i = 1969; i <= 1969; ++i) start[i] = 1;
		for (int i = 1994; i <= 2026; ++i) start[i] = 1;
		for (int i = 2036; i <= 2037; ++i) start[i] = 1;
		for (int i = 2042; i <= 2042; ++i) start[i] = 1;
		for (int i = 2048; i <= 2069; ++i) start[i] = 1;
		for (int i = 2074; i <= 2074; ++i) start[i] = 1;
		for (int i = 2084; i <= 2084; ++i) start[i] = 1;
		for (int i = 2088; i <= 2088; ++i) start[i] = 1;
		for (int i = 2112; i <= 2136; ++i) start[i] = 1;
		for (int i = 2208; i <= 2208; ++i) start[i] = 1;
		for (int i = 2210; i <= 2220; ++i) start[i] = 1;
		for (int i = 2308; i <= 2361; ++i) start[i] = 1;
		for (int i = 2365; i <= 2365; ++i) start[i] = 1;
		for (int i = 2384; i <= 2384; ++i) start[i] = 1;
		for (int i = 2392; i <= 2401; ++i) start[i] = 1;
		for (int i = 2417; i <= 2423; ++i) start[i] = 1;
		for (int i = 2425; i <= 2431; ++i) start[i] = 1;
		for (int i = 2437; i <= 2444; ++i) start[i] = 1;
		for (int i = 2447; i <= 2448; ++i) start[i] = 1;
		for (int i = 2451; i <= 2472; ++i) start[i] = 1;
		for (int i = 2474; i <= 2480; ++i) start[i] = 1;
		for (int i = 2482; i <= 2482; ++i) start[i] = 1;
		for (int i = 2486; i <= 2489; ++i) start[i] = 1;
		for (int i = 2493; i <= 2493; ++i) start[i] = 1;
		for (int i = 2510; i <= 2510; ++i) start[i] = 1;
		for (int i = 2524; i <= 2525; ++i) start[i] = 1;
		for (int i = 2527; i <= 2529; ++i) start[i] = 1;
		for (int i = 2544; i <= 2545; ++i) start[i] = 1;
		for (int i = 2565; i <= 2570; ++i) start[i] = 1;
		for (int i = 2575; i <= 2576; ++i) start[i] = 1;
		for (int i = 2579; i <= 2600; ++i) start[i] = 1;
		for (int i = 2602; i <= 2608; ++i) start[i] = 1;
		for (int i = 2610; i <= 2611; ++i) start[i] = 1;
		for (int i = 2613; i <= 2614; ++i) start[i] = 1;
		for (int i = 2616; i <= 2617; ++i) start[i] = 1;
		for (int i = 2649; i <= 2652; ++i) start[i] = 1;
		for (int i = 2654; i <= 2654; ++i) start[i] = 1;
		for (int i = 2674; i <= 2676; ++i) start[i] = 1;
		for (int i = 2693; i <= 2701; ++i) start[i] = 1;
		for (int i = 2703; i <= 2705; ++i) start[i] = 1;
		for (int i = 2707; i <= 2728; ++i) start[i] = 1;
		for (int i = 2730; i <= 2736; ++i) start[i] = 1;
		for (int i = 2738; i <= 2739; ++i) start[i] = 1;
		for (int i = 2741; i <= 2745; ++i) start[i] = 1;
		for (int i = 2749; i <= 2749; ++i) start[i] = 1;
		for (int i = 2768; i <= 2768; ++i) start[i] = 1;
		for (int i = 2784; i <= 2785; ++i) start[i] = 1;
		for (int i = 2821; i <= 2828; ++i) start[i] = 1;
		for (int i = 2831; i <= 2832; ++i) start[i] = 1;
		for (int i = 2835; i <= 2856; ++i) start[i] = 1;
		for (int i = 2858; i <= 2864; ++i) start[i] = 1;
		for (int i = 2866; i <= 2867; ++i) start[i] = 1;
		for (int i = 2869; i <= 2873; ++i) start[i] = 1;
		for (int i = 2877; i <= 2877; ++i) start[i] = 1;
		for (int i = 2908; i <= 2909; ++i) start[i] = 1;
		for (int i = 2911; i <= 2913; ++i) start[i] = 1;
		for (int i = 2929; i <= 2929; ++i) start[i] = 1;
		for (int i = 2947; i <= 2947; ++i) start[i] = 1;
		for (int i = 2949; i <= 2954; ++i) start[i] = 1;
		for (int i = 2958; i <= 2960; ++i) start[i] = 1;
		for (int i = 2962; i <= 2965; ++i) start[i] = 1;
		for (int i = 2969; i <= 2970; ++i) start[i] = 1;
		for (int i = 2972; i <= 2972; ++i) start[i] = 1;
		for (int i = 2974; i <= 2975; ++i) start[i] = 1;
		for (int i = 2979; i <= 2980; ++i) start[i] = 1;
		for (int i = 2984; i <= 2986; ++i) start[i] = 1;
		for (int i = 2990; i <= 3001; ++i) start[i] = 1;
		for (int i = 3024; i <= 3024; ++i) start[i] = 1;
		for (int i = 3077; i <= 3084; ++i) start[i] = 1;
		for (int i = 3086; i <= 3088; ++i) start[i] = 1;
		for (int i = 3090; i <= 3112; ++i) start[i] = 1;
		for (int i = 3114; i <= 3123; ++i) start[i] = 1;
		for (int i = 3125; i <= 3129; ++i) start[i] = 1;
		for (int i = 3133; i <= 3133; ++i) start[i] = 1;
		for (int i = 3160; i <= 3161; ++i) start[i] = 1;
		for (int i = 3168; i <= 3169; ++i) start[i] = 1;
		for (int i = 3205; i <= 3212; ++i) start[i] = 1;
		for (int i = 3214; i <= 3216; ++i) start[i] = 1;
		for (int i = 3218; i <= 3240; ++i) start[i] = 1;
		for (int i = 3242; i <= 3251; ++i) start[i] = 1;
		for (int i = 3253; i <= 3257; ++i) start[i] = 1;
		for (int i = 3261; i <= 3261; ++i) start[i] = 1;
		for (int i = 3294; i <= 3294; ++i) start[i] = 1;
		for (int i = 3296; i <= 3297; ++i) start[i] = 1;
		for (int i = 3313; i <= 3314; ++i) start[i] = 1;
		for (int i = 3333; i <= 3340; ++i) start[i] = 1;
		for (int i = 3342; i <= 3344; ++i) start[i] = 1;
		for (int i = 3346; i <= 3386; ++i) start[i] = 1;
		for (int i = 3389; i <= 3389; ++i) start[i] = 1;
		for (int i = 3406; i <= 3406; ++i) start[i] = 1;
		for (int i = 3424; i <= 3425; ++i) start[i] = 1;
		for (int i = 3450; i <= 3455; ++i) start[i] = 1;
		for (int i = 3461; i <= 3478; ++i) start[i] = 1;
		for (int i = 3482; i <= 3505; ++i) start[i] = 1;
		for (int i = 3507; i <= 3515; ++i) start[i] = 1;
		for (int i = 3517; i <= 3517; ++i) start[i] = 1;
		for (int i = 3520; i <= 3526; ++i) start[i] = 1;
		for (int i = 3585; i <= 3632; ++i) start[i] = 1;
		for (int i = 3634; i <= 3635; ++i) start[i] = 1;
		for (int i = 3648; i <= 3654; ++i) start[i] = 1;
		for (int i = 3713; i <= 3714; ++i) start[i] = 1;
		for (int i = 3716; i <= 3716; ++i) start[i] = 1;
		for (int i = 3719; i <= 3720; ++i) start[i] = 1;
		for (int i = 3722; i <= 3722; ++i) start[i] = 1;
		for (int i = 3725; i <= 3725; ++i) start[i] = 1;
		for (int i = 3732; i <= 3735; ++i) start[i] = 1;
		for (int i = 3737; i <= 3743; ++i) start[i] = 1;
		for (int i = 3745; i <= 3747; ++i) start[i] = 1;
		for (int i = 3749; i <= 3749; ++i) start[i] = 1;
		for (int i = 3751; i <= 3751; ++i) start[i] = 1;
		for (int i = 3754; i <= 3755; ++i) start[i] = 1;
		for (int i = 3757; i <= 3760; ++i) start[i] = 1;
		for (int i = 3762; i <= 3763; ++i) start[i] = 1;
		for (int i = 3773; i <= 3773; ++i) start[i] = 1;
		for (int i = 3776; i <= 3780; ++i) start[i] = 1;
		for (int i = 3782; i <= 3782; ++i) start[i] = 1;
		for (int i = 3804; i <= 3807; ++i) start[i] = 1;
		for (int i = 3840; i <= 3840; ++i) start[i] = 1;
		for (int i = 3904; i <= 3911; ++i) start[i] = 1;
		for (int i = 3913; i <= 3948; ++i) start[i] = 1;
		for (int i = 3976; i <= 3980; ++i) start[i] = 1;
		for (int i = 4096; i <= 4138; ++i) start[i] = 1;
		for (int i = 4159; i <= 4159; ++i) start[i] = 1;
		for (int i = 4176; i <= 4181; ++i) start[i] = 1;
		for (int i = 4186; i <= 4189; ++i) start[i] = 1;
		for (int i = 4193; i <= 4193; ++i) start[i] = 1;
		for (int i = 4197; i <= 4198; ++i) start[i] = 1;
		for (int i = 4206; i <= 4208; ++i) start[i] = 1;
		for (int i = 4213; i <= 4225; ++i) start[i] = 1;
		for (int i = 4238; i <= 4238; ++i) start[i] = 1;
		for (int i = 4256; i <= 4293; ++i) start[i] = 1;
		for (int i = 4295; i <= 4295; ++i) start[i] = 1;
		for (int i = 4301; i <= 4301; ++i) start[i] = 1;
		for (int i = 4304; i <= 4346; ++i) start[i] = 1;
		for (int i = 4348; i <= 4680; ++i) start[i] = 1;
		for (int i = 4682; i <= 4685; ++i) start[i] = 1;
		for (int i = 4688; i <= 4694; ++i) start[i] = 1;
		for (int i = 4696; i <= 4696; ++i) start[i] = 1;
		for (int i = 4698; i <= 4701; ++i) start[i] = 1;
		for (int i = 4704; i <= 4744; ++i) start[i] = 1;
		for (int i = 4746; i <= 4749; ++i) start[i] = 1;
		for (int i = 4752; i <= 4784; ++i) start[i] = 1;
		for (int i = 4786; i <= 4789; ++i) start[i] = 1;
		for (int i = 4792; i <= 4798; ++i) start[i] = 1;
		for (int i = 4800; i <= 4800; ++i) start[i] = 1;
		for (int i = 4802; i <= 4805; ++i) start[i] = 1;
		for (int i = 4808; i <= 4822; ++i) start[i] = 1;
		for (int i = 4824; i <= 4880; ++i) start[i] = 1;
		for (int i = 4882; i <= 4885; ++i) start[i] = 1;
		for (int i = 4888; i <= 4954; ++i) start[i] = 1;
		for (int i = 4992; i <= 5007; ++i) start[i] = 1;
		for (int i = 5024; i <= 5108; ++i) start[i] = 1;
		for (int i = 5121; i <= 5740; ++i) start[i] = 1;
		for (int i = 5743; i <= 5759; ++i) start[i] = 1;
		for (int i = 5761; i <= 5786; ++i) start[i] = 1;
		for (int i = 5792; i <= 5866; ++i) start[i] = 1;
		for (int i = 5870; i <= 5872; ++i) start[i] = 1;
		for (int i = 5888; i <= 5900; ++i) start[i] = 1;
		for (int i = 5902; i <= 5905; ++i) start[i] = 1;
		for (int i = 5920; i <= 5937; ++i) start[i] = 1;
		for (int i = 5952; i <= 5969; ++i) start[i] = 1;
		for (int i = 5984; i <= 5996; ++i) start[i] = 1;
		for (int i = 5998; i <= 6000; ++i) start[i] = 1;
		for (int i = 6016; i <= 6067; ++i) start[i] = 1;
		for (int i = 6103; i <= 6103; ++i) start[i] = 1;
		for (int i = 6108; i <= 6108; ++i) start[i] = 1;
		for (int i = 6176; i <= 6263; ++i) start[i] = 1;
		for (int i = 6272; i <= 6312; ++i) start[i] = 1;
		for (int i = 6314; i <= 6314; ++i) start[i] = 1;
		for (int i = 6320; i <= 6389; ++i) start[i] = 1;
		for (int i = 6400; i <= 6428; ++i) start[i] = 1;
		for (int i = 6480; i <= 6509; ++i) start[i] = 1;
		for (int i = 6512; i <= 6516; ++i) start[i] = 1;
		for (int i = 6528; i <= 6571; ++i) start[i] = 1;
		for (int i = 6593; i <= 6599; ++i) start[i] = 1;
		for (int i = 6656; i <= 6678; ++i) start[i] = 1;
		for (int i = 6688; i <= 6740; ++i) start[i] = 1;
		for (int i = 6823; i <= 6823; ++i) start[i] = 1;
		for (int i = 6917; i <= 6963; ++i) start[i] = 1;
		for (int i = 6981; i <= 6987; ++i) start[i] = 1;
		for (int i = 7043; i <= 7072; ++i) start[i] = 1;
		for (int i = 7086; i <= 7087; ++i) start[i] = 1;
		for (int i = 7098; i <= 7141; ++i) start[i] = 1;
		for (int i = 7168; i <= 7203; ++i) start[i] = 1;
		for (int i = 7245; i <= 7247; ++i) start[i] = 1;
		for (int i = 7258; i <= 7293; ++i) start[i] = 1;
		for (int i = 7401; i <= 7404; ++i) start[i] = 1;
		for (int i = 7406; i <= 7409; ++i) start[i] = 1;
		for (int i = 7413; i <= 7414; ++i) start[i] = 1;
		for (int i = 7424; i <= 7615; ++i) start[i] = 1;
		for (int i = 7680; i <= 7957; ++i) start[i] = 1;
		for (int i = 7960; i <= 7965; ++i) start[i] = 1;
		for (int i = 7968; i <= 8005; ++i) start[i] = 1;
		for (int i = 8008; i <= 8013; ++i) start[i] = 1;
		for (int i = 8016; i <= 8023; ++i) start[i] = 1;
		for (int i = 8025; i <= 8025; ++i) start[i] = 1;
		for (int i = 8027; i <= 8027; ++i) start[i] = 1;
		for (int i = 8029; i <= 8029; ++i) start[i] = 1;
		for (int i = 8031; i <= 8061; ++i) start[i] = 1;
		for (int i = 8064; i <= 8116; ++i) start[i] = 1;
		for (int i = 8118; i <= 8124; ++i) start[i] = 1;
		for (int i = 8126; i <= 8126; ++i) start[i] = 1;
		for (int i = 8130; i <= 8132; ++i) start[i] = 1;
		for (int i = 8134; i <= 8140; ++i) start[i] = 1;
		for (int i = 8144; i <= 8147; ++i) start[i] = 1;
		for (int i = 8150; i <= 8155; ++i) start[i] = 1;
		for (int i = 8160; i <= 8172; ++i) start[i] = 1;
		for (int i = 8178; i <= 8180; ++i) start[i] = 1;
		for (int i = 8182; i <= 8188; ++i) start[i] = 1;
		for (int i = 8305; i <= 8305; ++i) start[i] = 1;
		for (int i = 8319; i <= 8319; ++i) start[i] = 1;
		for (int i = 8336; i <= 8348; ++i) start[i] = 1;
		for (int i = 8450; i <= 8450; ++i) start[i] = 1;
		for (int i = 8455; i <= 8455; ++i) start[i] = 1;
		for (int i = 8458; i <= 8467; ++i) start[i] = 1;
		for (int i = 8469; i <= 8469; ++i) start[i] = 1;
		for (int i = 8473; i <= 8477; ++i) start[i] = 1;
		for (int i = 8484; i <= 8484; ++i) start[i] = 1;
		for (int i = 8486; i <= 8486; ++i) start[i] = 1;
		for (int i = 8488; i <= 8488; ++i) start[i] = 1;
		for (int i = 8490; i <= 8493; ++i) start[i] = 1;
		for (int i = 8495; i <= 8505; ++i) start[i] = 1;
		for (int i = 8508; i <= 8511; ++i) start[i] = 1;
		for (int i = 8517; i <= 8521; ++i) start[i] = 1;
		for (int i = 8526; i <= 8526; ++i) start[i] = 1;
		for (int i = 8544; i <= 8584; ++i) start[i] = 1;
		for (int i = 11264; i <= 11310; ++i) start[i] = 1;
		for (int i = 11312; i <= 11358; ++i) start[i] = 1;
		for (int i = 11360; i <= 11492; ++i) start[i] = 1;
		for (int i = 11499; i <= 11502; ++i) start[i] = 1;
		for (int i = 11506; i <= 11507; ++i) start[i] = 1;
		for (int i = 11520; i <= 11557; ++i) start[i] = 1;
		for (int i = 11559; i <= 11559; ++i) start[i] = 1;
		for (int i = 11565; i <= 11565; ++i) start[i] = 1;
		for (int i = 11568; i <= 11623; ++i) start[i] = 1;
		for (int i = 11631; i <= 11631; ++i) start[i] = 1;
		for (int i = 11648; i <= 11670; ++i) start[i] = 1;
		for (int i = 11680; i <= 11686; ++i) start[i] = 1;
		for (int i = 11688; i <= 11694; ++i) start[i] = 1;
		for (int i = 11696; i <= 11702; ++i) start[i] = 1;
		for (int i = 11704; i <= 11710; ++i) start[i] = 1;
		for (int i = 11712; i <= 11718; ++i) start[i] = 1;
		for (int i = 11720; i <= 11726; ++i) start[i] = 1;
		for (int i = 11728; i <= 11734; ++i) start[i] = 1;
		for (int i = 11736; i <= 11742; ++i) start[i] = 1;
		for (int i = 11823; i <= 11823; ++i) start[i] = 1;
		for (int i = 12293; i <= 12295; ++i) start[i] = 1;
		for (int i = 12321; i <= 12329; ++i) start[i] = 1;
		for (int i = 12337; i <= 12341; ++i) start[i] = 1;
		for (int i = 12344; i <= 12348; ++i) start[i] = 1;
		for (int i = 12353; i <= 12438; ++i) start[i] = 1;
		for (int i = 12445; i <= 12447; ++i) start[i] = 1;
		for (int i = 12449; i <= 12538; ++i) start[i] = 1;
		for (int i = 12540; i <= 12543; ++i) start[i] = 1;
		for (int i = 12549; i <= 12589; ++i) start[i] = 1;
		for (int i = 12593; i <= 12686; ++i) start[i] = 1;
		for (int i = 12704; i <= 12730; ++i) start[i] = 1;
		for (int i = 12784; i <= 12799; ++i) start[i] = 1;
		for (int i = 13312; i <= 19893; ++i) start[i] = 1;
		for (int i = 19968; i <= 40908; ++i) start[i] = 1;
		for (int i = 40960; i <= 42124; ++i) start[i] = 1;
		for (int i = 42192; i <= 42237; ++i) start[i] = 1;
		for (int i = 42240; i <= 42508; ++i) start[i] = 1;
		for (int i = 42512; i <= 42527; ++i) start[i] = 1;
		for (int i = 42538; i <= 42539; ++i) start[i] = 1;
		for (int i = 42560; i <= 42606; ++i) start[i] = 1;
		for (int i = 42623; i <= 42647; ++i) start[i] = 1;
		for (int i = 42656; i <= 42735; ++i) start[i] = 1;
		for (int i = 42775; i <= 42783; ++i) start[i] = 1;
		for (int i = 42786; i <= 42888; ++i) start[i] = 1;
		for (int i = 42891; i <= 42894; ++i) start[i] = 1;
		for (int i = 42896; i <= 42899; ++i) start[i] = 1;
		for (int i = 42912; i <= 42922; ++i) start[i] = 1;
		for (int i = 43000; i <= 43009; ++i) start[i] = 1;
		for (int i = 43011; i <= 43013; ++i) start[i] = 1;
		for (int i = 43015; i <= 43018; ++i) start[i] = 1;
		for (int i = 43020; i <= 43042; ++i) start[i] = 1;
		for (int i = 43072; i <= 43123; ++i) start[i] = 1;
		for (int i = 43138; i <= 43187; ++i) start[i] = 1;
		for (int i = 43250; i <= 43255; ++i) start[i] = 1;
		for (int i = 43259; i <= 43259; ++i) start[i] = 1;
		for (int i = 43274; i <= 43301; ++i) start[i] = 1;
		for (int i = 43312; i <= 43334; ++i) start[i] = 1;
		for (int i = 43360; i <= 43388; ++i) start[i] = 1;
		for (int i = 43396; i <= 43442; ++i) start[i] = 1;
		for (int i = 43471; i <= 43471; ++i) start[i] = 1;
		for (int i = 43520; i <= 43560; ++i) start[i] = 1;
		for (int i = 43584; i <= 43586; ++i) start[i] = 1;
		for (int i = 43588; i <= 43595; ++i) start[i] = 1;
		for (int i = 43616; i <= 43638; ++i) start[i] = 1;
		for (int i = 43642; i <= 43642; ++i) start[i] = 1;
		for (int i = 43648; i <= 43695; ++i) start[i] = 1;
		for (int i = 43697; i <= 43697; ++i) start[i] = 1;
		for (int i = 43701; i <= 43702; ++i) start[i] = 1;
		for (int i = 43705; i <= 43709; ++i) start[i] = 1;
		for (int i = 43712; i <= 43712; ++i) start[i] = 1;
		for (int i = 43714; i <= 43714; ++i) start[i] = 1;
		for (int i = 43739; i <= 43741; ++i) start[i] = 1;
		for (int i = 43744; i <= 43754; ++i) start[i] = 1;
		for (int i = 43762; i <= 43764; ++i) start[i] = 1;
		for (int i = 43777; i <= 43782; ++i) start[i] = 1;
		for (int i = 43785; i <= 43790; ++i) start[i] = 1;
		for (int i = 43793; i <= 43798; ++i) start[i] = 1;
		for (int i = 43808; i <= 43814; ++i) start[i] = 1;
		for (int i = 43816; i <= 43822; ++i) start[i] = 1;
		for (int i = 43968; i <= 44002; ++i) start[i] = 1;
		for (int i = 44032; i <= 55203; ++i) start[i] = 1;
		for (int i = 55216; i <= 55238; ++i) start[i] = 1;
		for (int i = 55243; i <= 55291; ++i) start[i] = 1;
		for (int i = 63744; i <= 64109; ++i) start[i] = 1;
		for (int i = 64112; i <= 64217; ++i) start[i] = 1;
		for (int i = 64256; i <= 64262; ++i) start[i] = 1;
		for (int i = 64275; i <= 64279; ++i) start[i] = 1;
		for (int i = 64285; i <= 64285; ++i) start[i] = 1;
		for (int i = 64287; i <= 64296; ++i) start[i] = 1;
		for (int i = 64298; i <= 64310; ++i) start[i] = 1;
		for (int i = 64312; i <= 64316; ++i) start[i] = 1;
		for (int i = 64318; i <= 64318; ++i) start[i] = 1;
		for (int i = 64320; i <= 64321; ++i) start[i] = 1;
		for (int i = 64323; i <= 64324; ++i) start[i] = 1;
		for (int i = 64326; i <= 64433; ++i) start[i] = 1;
		for (int i = 64467; i <= 64829; ++i) start[i] = 1;
		for (int i = 64848; i <= 64911; ++i) start[i] = 1;
		for (int i = 64914; i <= 64967; ++i) start[i] = 1;
		for (int i = 65008; i <= 65019; ++i) start[i] = 1;
		for (int i = 65136; i <= 65140; ++i) start[i] = 1;
		for (int i = 65142; i <= 65276; ++i) start[i] = 1;
		for (int i = 65313; i <= 65338; ++i) start[i] = 1;
		for (int i = 65345; i <= 65370; ++i) start[i] = 1;
		for (int i = 65382; i <= 65470; ++i) start[i] = 1;
		for (int i = 65474; i <= 65479; ++i) start[i] = 1;
		for (int i = 65482; i <= 65487; ++i) start[i] = 1;
		for (int i = 65490; i <= 65495; ++i) start[i] = 1;
		for (int i = 65498; i <= 65500; ++i) start[i] = 1;
		for (int i = 48; i <= 57; ++i) start[i] = 28;
		start[34] = 7; 
		start[39] = 9; 
		start[46] = 29; 
		start[64] = 30; 
		start[91] = 14; 
		start[93] = 15; 
		start[40] = 16; 
		start[41] = 17; 
		start[33] = 31; 
		start[45] = 18; 
		start[124] = 51; 
		start[60] = 32; 
		start[62] = 33; 
		start[61] = 52; 
		start[59] = 23; 
		start[47] = 53; 
		start[123] = 38; 
		start[125] = 39; 
		start[58] = 40; 
		start[44] = 41; 
		start[63] = 42; 
		start[43] = 43; 
		start[42] = 44; 
		start[37] = 45; 
		start[38] = 54; 
		start[126] = 48; 
		start[35] = 49; 
		start[94] = 50; 
		start[Buffer.EOF] = -1;

	}
	
	public Scanner (string fileName) {
		try {
			Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			buffer = new Buffer(stream, false);
			Init();
		} catch (IOException) {
			throw new FatalError("Cannot open file " + fileName);
		}
	}
	
	public Scanner (Stream s) {
		buffer = new Buffer(s, true);
		Init();
	}
	
	void Init() {
		pos = -1; line = 1; col = 0; charPos = -1;
		oldEols = 0;
		NextCh();
		if (ch == 0xEF) { // check optional byte order mark for UTF-8
			NextCh(); int ch1 = ch;
			NextCh(); int ch2 = ch;
			if (ch1 != 0xBB || ch2 != 0xBF) {
				throw new FatalError(String.Format("illegal byte order mark: EF {0,2:X} {1,2:X}", ch1, ch2));
			}
			buffer = new UTF8Buffer(buffer); col = 0; charPos = -1;
			NextCh();
		}
		pt = tokens = new Token();  // first token is a dummy
	}
	
	void NextCh() {
		if (oldEols > 0) { ch = EOL; oldEols--; } 
		else {
			pos = buffer.Pos;
			// buffer reads unicode chars, if UTF8 has been detected
			ch = buffer.Read(); col++; charPos++;
			// replace isolated '\r' by '\n' in order to make
			// eol handling uniform across Windows, Unix and Mac
			if (ch == '\r' && buffer.Peek() != '\n') ch = EOL;
			if (ch == EOL) { line++; col = 0; }
		}

	}

	void AddCh() {
		if (tlen >= tval.Length) {
			char[] newBuf = new char[2 * tval.Length];
			Array.Copy(tval, 0, newBuf, 0, tval.Length);
			tval = newBuf;
		}
		if (ch != Buffer.EOF) {
			tval[tlen++] = (char) ch;
			NextCh();
		}
	}




	void CheckLiteral() {
		switch (t.val) {
			case "class": t.kind = 25; break;
			case "constructor": t.kind = 26; break;
			case "def": t.kind = 27; break;
			case "extends": t.kind = 28; break;
			case "if": t.kind = 29; break;
			case "elseif": t.kind = 30; break;
			case "else": t.kind = 31; break;
			case "while": t.kind = 32; break;
			case "for": t.kind = 33; break;
			case "import": t.kind = 34; break;
			case "prefix": t.kind = 35; break;
			case "from": t.kind = 36; break;
			case "break": t.kind = 37; break;
			case "continue": t.kind = 38; break;
			case "static": t.kind = 39; break;
			case "local": t.kind = 40; break;
			case "true": t.kind = 41; break;
			case "false": t.kind = 42; break;
			case "null": t.kind = 43; break;
			case "return": t.kind = 44; break;
			case "public": t.kind = 49; break;
			case "private": t.kind = 50; break;
			case "protected": t.kind = 51; break;
			case "in": t.kind = 62; break;
			default: break;
		}
	}

	Token NextToken() {
		while (ch == ' ' ||
			ch >= 9 && ch <= 10 || ch == 13
		) NextCh();

		int recKind = noSym;
		int recEnd = pos;
		t = new Token();
		t.pos = pos; t.col = col; t.line = line; t.charPos = charPos;
		int state;
		if (start.ContainsKey(ch)) { state = (int) start[ch]; }
		else { state = 0; }
		tlen = 0; AddCh();
		
		switch (state) {
			case -1: { t.kind = eofSym; break; } // NextCh already done
			case 0: {
				if (recKind != noSym) {
					tlen = recEnd - t.pos;
					SetScannerBehindT();
				}
				t.kind = recKind; break;
			} // NextCh already done
			case 1:
				recEnd = pos; recKind = 1;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z' || ch == 170 || ch == 181 || ch == 186 || ch >= 192 && ch <= 214 || ch >= 216 && ch <= 246 || ch >= 248 && ch <= 705 || ch >= 710 && ch <= 721 || ch >= 736 && ch <= 740 || ch == 748 || ch == 750 || ch >= 768 && ch <= 884 || ch >= 886 && ch <= 887 || ch >= 890 && ch <= 893 || ch == 902 || ch >= 904 && ch <= 906 || ch == 908 || ch >= 910 && ch <= 929 || ch >= 931 && ch <= 1013 || ch >= 1015 && ch <= 1153 || ch >= 1155 && ch <= 1159 || ch >= 1162 && ch <= 1319 || ch >= 1329 && ch <= 1366 || ch == 1369 || ch >= 1377 && ch <= 1415 || ch >= 1425 && ch <= 1469 || ch == 1471 || ch >= 1473 && ch <= 1474 || ch >= 1476 && ch <= 1477 || ch == 1479 || ch >= 1488 && ch <= 1514 || ch >= 1520 && ch <= 1522 || ch >= 1552 && ch <= 1562 || ch >= 1568 && ch <= 1641 || ch >= 1646 && ch <= 1747 || ch >= 1749 && ch <= 1756 || ch >= 1759 && ch <= 1768 || ch >= 1770 && ch <= 1788 || ch == 1791 || ch >= 1808 && ch <= 1866 || ch >= 1869 && ch <= 1969 || ch >= 1984 && ch <= 2037 || ch == 2042 || ch >= 2048 && ch <= 2093 || ch >= 2112 && ch <= 2139 || ch == 2208 || ch >= 2210 && ch <= 2220 || ch >= 2276 && ch <= 2302 || ch >= 2304 && ch <= 2403 || ch >= 2406 && ch <= 2415 || ch >= 2417 && ch <= 2423 || ch >= 2425 && ch <= 2431 || ch >= 2433 && ch <= 2435 || ch >= 2437 && ch <= 2444 || ch >= 2447 && ch <= 2448 || ch >= 2451 && ch <= 2472 || ch >= 2474 && ch <= 2480 || ch == 2482 || ch >= 2486 && ch <= 2489 || ch >= 2492 && ch <= 2500 || ch >= 2503 && ch <= 2504 || ch >= 2507 && ch <= 2510 || ch == 2519 || ch >= 2524 && ch <= 2525 || ch >= 2527 && ch <= 2531 || ch >= 2534 && ch <= 2545 || ch >= 2561 && ch <= 2563 || ch >= 2565 && ch <= 2570 || ch >= 2575 && ch <= 2576 || ch >= 2579 && ch <= 2600 || ch >= 2602 && ch <= 2608 || ch >= 2610 && ch <= 2611 || ch >= 2613 && ch <= 2614 || ch >= 2616 && ch <= 2617 || ch == 2620 || ch >= 2622 && ch <= 2626 || ch >= 2631 && ch <= 2632 || ch >= 2635 && ch <= 2637 || ch == 2641 || ch >= 2649 && ch <= 2652 || ch == 2654 || ch >= 2662 && ch <= 2677 || ch >= 2689 && ch <= 2691 || ch >= 2693 && ch <= 2701 || ch >= 2703 && ch <= 2705 || ch >= 2707 && ch <= 2728 || ch >= 2730 && ch <= 2736 || ch >= 2738 && ch <= 2739 || ch >= 2741 && ch <= 2745 || ch >= 2748 && ch <= 2757 || ch >= 2759 && ch <= 2761 || ch >= 2763 && ch <= 2765 || ch == 2768 || ch >= 2784 && ch <= 2787 || ch >= 2790 && ch <= 2799 || ch >= 2817 && ch <= 2819 || ch >= 2821 && ch <= 2828 || ch >= 2831 && ch <= 2832 || ch >= 2835 && ch <= 2856 || ch >= 2858 && ch <= 2864 || ch >= 2866 && ch <= 2867 || ch >= 2869 && ch <= 2873 || ch >= 2876 && ch <= 2884 || ch >= 2887 && ch <= 2888 || ch >= 2891 && ch <= 2893 || ch >= 2902 && ch <= 2903 || ch >= 2908 && ch <= 2909 || ch >= 2911 && ch <= 2915 || ch >= 2918 && ch <= 2927 || ch == 2929 || ch >= 2946 && ch <= 2947 || ch >= 2949 && ch <= 2954 || ch >= 2958 && ch <= 2960 || ch >= 2962 && ch <= 2965 || ch >= 2969 && ch <= 2970 || ch == 2972 || ch >= 2974 && ch <= 2975 || ch >= 2979 && ch <= 2980 || ch >= 2984 && ch <= 2986 || ch >= 2990 && ch <= 3001 || ch >= 3006 && ch <= 3010 || ch >= 3014 && ch <= 3016 || ch >= 3018 && ch <= 3021 || ch == 3024 || ch == 3031 || ch >= 3046 && ch <= 3055 || ch >= 3073 && ch <= 3075 || ch >= 3077 && ch <= 3084 || ch >= 3086 && ch <= 3088 || ch >= 3090 && ch <= 3112 || ch >= 3114 && ch <= 3123 || ch >= 3125 && ch <= 3129 || ch >= 3133 && ch <= 3140 || ch >= 3142 && ch <= 3144 || ch >= 3146 && ch <= 3149 || ch >= 3157 && ch <= 3158 || ch >= 3160 && ch <= 3161 || ch >= 3168 && ch <= 3171 || ch >= 3174 && ch <= 3183 || ch >= 3202 && ch <= 3203 || ch >= 3205 && ch <= 3212 || ch >= 3214 && ch <= 3216 || ch >= 3218 && ch <= 3240 || ch >= 3242 && ch <= 3251 || ch >= 3253 && ch <= 3257 || ch >= 3260 && ch <= 3268 || ch >= 3270 && ch <= 3272 || ch >= 3274 && ch <= 3277 || ch >= 3285 && ch <= 3286 || ch == 3294 || ch >= 3296 && ch <= 3299 || ch >= 3302 && ch <= 3311 || ch >= 3313 && ch <= 3314 || ch >= 3330 && ch <= 3331 || ch >= 3333 && ch <= 3340 || ch >= 3342 && ch <= 3344 || ch >= 3346 && ch <= 3386 || ch >= 3389 && ch <= 3396 || ch >= 3398 && ch <= 3400 || ch >= 3402 && ch <= 3406 || ch == 3415 || ch >= 3424 && ch <= 3427 || ch >= 3430 && ch <= 3439 || ch >= 3450 && ch <= 3455 || ch >= 3458 && ch <= 3459 || ch >= 3461 && ch <= 3478 || ch >= 3482 && ch <= 3505 || ch >= 3507 && ch <= 3515 || ch == 3517 || ch >= 3520 && ch <= 3526 || ch == 3530 || ch >= 3535 && ch <= 3540 || ch == 3542 || ch >= 3544 && ch <= 3551 || ch >= 3570 && ch <= 3571 || ch >= 3585 && ch <= 3642 || ch >= 3648 && ch <= 3662 || ch >= 3664 && ch <= 3673 || ch >= 3713 && ch <= 3714 || ch == 3716 || ch >= 3719 && ch <= 3720 || ch == 3722 || ch == 3725 || ch >= 3732 && ch <= 3735 || ch >= 3737 && ch <= 3743 || ch >= 3745 && ch <= 3747 || ch == 3749 || ch == 3751 || ch >= 3754 && ch <= 3755 || ch >= 3757 && ch <= 3769 || ch >= 3771 && ch <= 3773 || ch >= 3776 && ch <= 3780 || ch == 3782 || ch >= 3784 && ch <= 3789 || ch >= 3792 && ch <= 3801 || ch >= 3804 && ch <= 3807 || ch == 3840 || ch >= 3864 && ch <= 3865 || ch >= 3872 && ch <= 3881 || ch == 3893 || ch == 3895 || ch == 3897 || ch >= 3902 && ch <= 3911 || ch >= 3913 && ch <= 3948 || ch >= 3953 && ch <= 3972 || ch >= 3974 && ch <= 3991 || ch >= 3993 && ch <= 4028 || ch == 4038 || ch >= 4096 && ch <= 4169 || ch >= 4176 && ch <= 4253 || ch >= 4256 && ch <= 4293 || ch == 4295 || ch == 4301 || ch >= 4304 && ch <= 4346 || ch >= 4348 && ch <= 4680 || ch >= 4682 && ch <= 4685 || ch >= 4688 && ch <= 4694 || ch == 4696 || ch >= 4698 && ch <= 4701 || ch >= 4704 && ch <= 4744 || ch >= 4746 && ch <= 4749 || ch >= 4752 && ch <= 4784 || ch >= 4786 && ch <= 4789 || ch >= 4792 && ch <= 4798 || ch == 4800 || ch >= 4802 && ch <= 4805 || ch >= 4808 && ch <= 4822 || ch >= 4824 && ch <= 4880 || ch >= 4882 && ch <= 4885 || ch >= 4888 && ch <= 4954 || ch >= 4957 && ch <= 4959 || ch >= 4992 && ch <= 5007 || ch >= 5024 && ch <= 5108 || ch >= 5121 && ch <= 5740 || ch >= 5743 && ch <= 5759 || ch >= 5761 && ch <= 5786 || ch >= 5792 && ch <= 5866 || ch >= 5870 && ch <= 5872 || ch >= 5888 && ch <= 5900 || ch >= 5902 && ch <= 5908 || ch >= 5920 && ch <= 5940 || ch >= 5952 && ch <= 5971 || ch >= 5984 && ch <= 5996 || ch >= 5998 && ch <= 6000 || ch >= 6002 && ch <= 6003 || ch >= 6016 && ch <= 6099 || ch == 6103 || ch >= 6108 && ch <= 6109 || ch >= 6112 && ch <= 6121 || ch >= 6155 && ch <= 6157 || ch >= 6160 && ch <= 6169 || ch >= 6176 && ch <= 6263 || ch >= 6272 && ch <= 6314 || ch >= 6320 && ch <= 6389 || ch >= 6400 && ch <= 6428 || ch >= 6432 && ch <= 6443 || ch >= 6448 && ch <= 6459 || ch >= 6470 && ch <= 6509 || ch >= 6512 && ch <= 6516 || ch >= 6528 && ch <= 6571 || ch >= 6576 && ch <= 6601 || ch >= 6608 && ch <= 6617 || ch >= 6656 && ch <= 6683 || ch >= 6688 && ch <= 6750 || ch >= 6752 && ch <= 6780 || ch >= 6783 && ch <= 6793 || ch >= 6800 && ch <= 6809 || ch == 6823 || ch >= 6912 && ch <= 6987 || ch >= 6992 && ch <= 7001 || ch >= 7019 && ch <= 7027 || ch >= 7040 && ch <= 7155 || ch >= 7168 && ch <= 7223 || ch >= 7232 && ch <= 7241 || ch >= 7245 && ch <= 7293 || ch >= 7376 && ch <= 7378 || ch >= 7380 && ch <= 7414 || ch >= 7424 && ch <= 7654 || ch >= 7676 && ch <= 7957 || ch >= 7960 && ch <= 7965 || ch >= 7968 && ch <= 8005 || ch >= 8008 && ch <= 8013 || ch >= 8016 && ch <= 8023 || ch == 8025 || ch == 8027 || ch == 8029 || ch >= 8031 && ch <= 8061 || ch >= 8064 && ch <= 8116 || ch >= 8118 && ch <= 8124 || ch == 8126 || ch >= 8130 && ch <= 8132 || ch >= 8134 && ch <= 8140 || ch >= 8144 && ch <= 8147 || ch >= 8150 && ch <= 8155 || ch >= 8160 && ch <= 8172 || ch >= 8178 && ch <= 8180 || ch >= 8182 && ch <= 8188 || ch >= 8204 && ch <= 8205 || ch >= 8255 && ch <= 8256 || ch == 8276 || ch == 8305 || ch == 8319 || ch >= 8336 && ch <= 8348 || ch >= 8400 && ch <= 8412 || ch == 8417 || ch >= 8421 && ch <= 8432 || ch == 8450 || ch == 8455 || ch >= 8458 && ch <= 8467 || ch == 8469 || ch >= 8473 && ch <= 8477 || ch == 8484 || ch == 8486 || ch == 8488 || ch >= 8490 && ch <= 8493 || ch >= 8495 && ch <= 8505 || ch >= 8508 && ch <= 8511 || ch >= 8517 && ch <= 8521 || ch == 8526 || ch >= 8544 && ch <= 8584 || ch >= 11264 && ch <= 11310 || ch >= 11312 && ch <= 11358 || ch >= 11360 && ch <= 11492 || ch >= 11499 && ch <= 11507 || ch >= 11520 && ch <= 11557 || ch == 11559 || ch == 11565 || ch >= 11568 && ch <= 11623 || ch == 11631 || ch >= 11647 && ch <= 11670 || ch >= 11680 && ch <= 11686 || ch >= 11688 && ch <= 11694 || ch >= 11696 && ch <= 11702 || ch >= 11704 && ch <= 11710 || ch >= 11712 && ch <= 11718 || ch >= 11720 && ch <= 11726 || ch >= 11728 && ch <= 11734 || ch >= 11736 && ch <= 11742 || ch >= 11744 && ch <= 11775 || ch == 11823 || ch >= 12293 && ch <= 12295 || ch >= 12321 && ch <= 12335 || ch >= 12337 && ch <= 12341 || ch >= 12344 && ch <= 12348 || ch >= 12353 && ch <= 12438 || ch >= 12441 && ch <= 12442 || ch >= 12445 && ch <= 12447 || ch >= 12449 && ch <= 12538 || ch >= 12540 && ch <= 12543 || ch >= 12549 && ch <= 12589 || ch >= 12593 && ch <= 12686 || ch >= 12704 && ch <= 12730 || ch >= 12784 && ch <= 12799 || ch >= 13312 && ch <= 19893 || ch >= 19968 && ch <= 40908 || ch >= 40960 && ch <= 42124 || ch >= 42192 && ch <= 42237 || ch >= 42240 && ch <= 42508 || ch >= 42512 && ch <= 42539 || ch >= 42560 && ch <= 42607 || ch >= 42612 && ch <= 42621 || ch >= 42623 && ch <= 42647 || ch >= 42655 && ch <= 42737 || ch >= 42775 && ch <= 42783 || ch >= 42786 && ch <= 42888 || ch >= 42891 && ch <= 42894 || ch >= 42896 && ch <= 42899 || ch >= 42912 && ch <= 42922 || ch >= 43000 && ch <= 43047 || ch >= 43072 && ch <= 43123 || ch >= 43136 && ch <= 43204 || ch >= 43216 && ch <= 43225 || ch >= 43232 && ch <= 43255 || ch == 43259 || ch >= 43264 && ch <= 43309 || ch >= 43312 && ch <= 43347 || ch >= 43360 && ch <= 43388 || ch >= 43392 && ch <= 43456 || ch >= 43471 && ch <= 43481 || ch >= 43520 && ch <= 43574 || ch >= 43584 && ch <= 43597 || ch >= 43600 && ch <= 43609 || ch >= 43616 && ch <= 43638 || ch >= 43642 && ch <= 43643 || ch >= 43648 && ch <= 43714 || ch >= 43739 && ch <= 43741 || ch >= 43744 && ch <= 43759 || ch >= 43762 && ch <= 43766 || ch >= 43777 && ch <= 43782 || ch >= 43785 && ch <= 43790 || ch >= 43793 && ch <= 43798 || ch >= 43808 && ch <= 43814 || ch >= 43816 && ch <= 43822 || ch >= 43968 && ch <= 44010 || ch >= 44012 && ch <= 44013 || ch >= 44016 && ch <= 44025 || ch >= 44032 && ch <= 55203 || ch >= 55216 && ch <= 55238 || ch >= 55243 && ch <= 55291 || ch >= 63744 && ch <= 64109 || ch >= 64112 && ch <= 64217 || ch >= 64256 && ch <= 64262 || ch >= 64275 && ch <= 64279 || ch >= 64285 && ch <= 64296 || ch >= 64298 && ch <= 64310 || ch >= 64312 && ch <= 64316 || ch == 64318 || ch >= 64320 && ch <= 64321 || ch >= 64323 && ch <= 64324 || ch >= 64326 && ch <= 64433 || ch >= 64467 && ch <= 64829 || ch >= 64848 && ch <= 64911 || ch >= 64914 && ch <= 64967 || ch >= 65008 && ch <= 65019 || ch >= 65024 && ch <= 65039 || ch >= 65056 && ch <= 65062 || ch >= 65075 && ch <= 65076 || ch >= 65101 && ch <= 65103 || ch >= 65136 && ch <= 65140 || ch >= 65142 && ch <= 65276 || ch >= 65296 && ch <= 65305 || ch >= 65313 && ch <= 65338 || ch == 65343 || ch >= 65345 && ch <= 65370 || ch >= 65382 && ch <= 65470 || ch >= 65474 && ch <= 65479 || ch >= 65482 && ch <= 65487 || ch >= 65490 && ch <= 65495 || ch >= 65498 && ch <= 65500) {AddCh(); goto case 1;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 2:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 3;}
				else {goto case 0;}
			case 3:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 3;}
				else if (ch == 'E' || ch == 'e') {AddCh(); goto case 4;}
				else {t.kind = 3; break;}
			case 4:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 6;}
				else if (ch == '+' || ch == '-') {AddCh(); goto case 5;}
				else {goto case 0;}
			case 5:
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 6;}
				else {goto case 0;}
			case 6:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 6;}
				else {t.kind = 3; break;}
			case 7:
				if (ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 7;}
				else if (ch == '"') {AddCh(); goto case 8;}
				else if (ch == 92) {AddCh(); goto case 34;}
				else {goto case 0;}
			case 8:
				{t.kind = 4; break;}
			case 9:
				if (ch <= '&' || ch >= '(' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 10;}
				else if (ch == 92) {AddCh(); goto case 35;}
				else {goto case 0;}
			case 10:
				if (ch == 39) {AddCh(); goto case 11;}
				else {goto case 0;}
			case 11:
				{t.kind = 5; break;}
			case 12:
				{t.kind = 7; break;}
			case 13:
				{t.kind = 9; break;}
			case 14:
				{t.kind = 10; break;}
			case 15:
				{t.kind = 11; break;}
			case 16:
				{t.kind = 12; break;}
			case 17:
				{t.kind = 13; break;}
			case 18:
				{t.kind = 15; break;}
			case 19:
				{t.kind = 19; break;}
			case 20:
				{t.kind = 20; break;}
			case 21:
				{t.kind = 21; break;}
			case 22:
				{t.kind = 22; break;}
			case 23:
				{t.kind = 23; break;}
			case 24:
				{t.kind = 24; break;}
			case 25:
				recEnd = pos; recKind = 66;
				if (ch <= 9 || ch >= 11 && ch <= 65535) {AddCh(); goto case 25;}
				else {t.kind = 66; break;}
			case 26:
				if (ch <= ')' || ch >= '+' && ch <= 65535) {AddCh(); goto case 26;}
				else if (ch == '*') {AddCh(); goto case 36;}
				else {goto case 0;}
			case 27:
				{t.kind = 67; break;}
			case 28:
				recEnd = pos; recKind = 2;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 28;}
				else if (ch == '.') {AddCh(); goto case 2;}
				else if (ch == 'L') {AddCh(); goto case 12;}
				else {t.kind = 2; break;}
			case 29:
				recEnd = pos; recKind = 6;
				if (ch == '.') {AddCh(); goto case 24;}
				else {t.kind = 6; break;}
			case 30:
				recEnd = pos; recKind = 8;
				if (ch == '@') {AddCh(); goto case 13;}
				else {t.kind = 8; break;}
			case 31:
				recEnd = pos; recKind = 14;
				if (ch == '=') {AddCh(); goto case 22;}
				else {t.kind = 14; break;}
			case 32:
				recEnd = pos; recKind = 17;
				if (ch == '=') {AddCh(); goto case 19;}
				else {t.kind = 17; break;}
			case 33:
				recEnd = pos; recKind = 18;
				if (ch == '=') {AddCh(); goto case 20;}
				else {t.kind = 18; break;}
			case 34:
				if (ch == '"' || ch == 39 || ch == '0' || ch == 92 || ch >= 'a' && ch <= 'b' || ch == 'f' || ch == 'n' || ch == 'r' || ch >= 't' && ch <= 'v') {AddCh(); goto case 7;}
				else {goto case 0;}
			case 35:
				if (ch == 39) {AddCh(); goto case 37;}
				else if (ch == '"' || ch == '0' || ch == 92 || ch >= 'a' && ch <= 'b' || ch == 'f' || ch == 'n' || ch == 'r' || ch >= 't' && ch <= 'v') {AddCh(); goto case 10;}
				else {goto case 0;}
			case 36:
				if (ch <= ')' || ch >= '+' && ch <= '.' || ch >= '0' && ch <= 65535) {AddCh(); goto case 26;}
				else if (ch == '/') {AddCh(); goto case 27;}
				else if (ch == '*') {AddCh(); goto case 36;}
				else {goto case 0;}
			case 37:
				recEnd = pos; recKind = 5;
				if (ch == 39) {AddCh(); goto case 11;}
				else {t.kind = 5; break;}
			case 38:
				{t.kind = 46; break;}
			case 39:
				{t.kind = 47; break;}
			case 40:
				{t.kind = 48; break;}
			case 41:
				{t.kind = 52; break;}
			case 42:
				{t.kind = 53; break;}
			case 43:
				{t.kind = 54; break;}
			case 44:
				{t.kind = 55; break;}
			case 45:
				{t.kind = 57; break;}
			case 46:
				{t.kind = 58; break;}
			case 47:
				{t.kind = 59; break;}
			case 48:
				{t.kind = 60; break;}
			case 49:
				{t.kind = 61; break;}
			case 50:
				{t.kind = 64; break;}
			case 51:
				recEnd = pos; recKind = 16;
				if (ch == '|') {AddCh(); goto case 47;}
				else {t.kind = 16; break;}
			case 52:
				recEnd = pos; recKind = 45;
				if (ch == '=') {AddCh(); goto case 21;}
				else {t.kind = 45; break;}
			case 53:
				recEnd = pos; recKind = 56;
				if (ch == '/') {AddCh(); goto case 25;}
				else if (ch == '*') {AddCh(); goto case 26;}
				else {t.kind = 56; break;}
			case 54:
				recEnd = pos; recKind = 63;
				if (ch == '&') {AddCh(); goto case 46;}
				else {t.kind = 63; break;}

		}
		t.val = new String(tval, 0, tlen);
		return t;
	}
	
	private void SetScannerBehindT() {
		buffer.Pos = t.pos;
		NextCh();
		line = t.line; col = t.col; charPos = t.charPos;
		for (int i = 0; i < tlen; i++) NextCh();
	}
	
	// get the next token (possibly a token already seen during peeking)
	public Token Scan () {
		if (tokens.next == null) {
			return NextToken();
		} else {
			pt = tokens = tokens.next;
			return tokens;
		}
	}

	// peek for the next token, ignore pragmas
	public Token Peek () {
		do {
			if (pt.next == null) {
				pt.next = NextToken();
			}
			pt = pt.next;
		} while (pt.kind > maxT); // skip pragmas
	
		return pt;
	}

	// make sure that peeking starts at the current scan position
	public void ResetPeek () { pt = tokens; }

} // end Scanner
}