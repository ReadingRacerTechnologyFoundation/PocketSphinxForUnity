/*
 * Copyright (c) 2025 Reading Racer Technology Foundation
 *
 * This file is part of ReadingRacerTechnologyFoundation/PocketSphinxForUnity
 *
 * SPDX-License-Identifier: LGPL-3.0-or-later
 *
 * This software is distributed without any warranty.
 * See the LICENSE file in the project root for full terms.
 *
 * This source may contain or make use of third-party components,
 * including PocketSphinx and SphinxBase, which are licensed separately.
 * See THIRD_PARTY_LICENSES.txt for details.
 */
using System.Collections.ObjectModel;
using System;

namespace Rrtf.Sphinx.EnglishPhonemes
{
	public class English{
		
		/* Context definitions */
		static readonly string Anything = string.Empty;    /* No context requirement */
		const string Nothing = " ";    /* Context is beginning or end of word */
		
		/* Phoneme definitions */
		const string Pause = " ";      /* Short silence */
		static readonly string Silent = string.Empty;      /* No phonemes */
		
		/* Phoneme definitions - 41 strings for phonemes in English
AY, AW, OY, AND WH need two unicode chars to make std IPA representation*/
		
		// andersw: change to use space-terminated CMU phonemes, not IPA symbols
		const string IY = "IY ";
		const string IH = "IH ";
		const string EY = "EY ";
		const string EH = "EH ";
		const string AE = "AE ";
		
		const string AA = "AA ";
		const string AO = "AO ";
		const string OW = "OW ";
		const string UH = "UH ";
		const string UW = "UW ";
		
		const string ER = "ER ";
		const string AX = "AH ";	// andersw: AH used for AX in our "small phoneset" dictionary
		const string AH = "AH ";
		const string AY = "AY ";
		const string AW = "AW ";
		
		const string OY = "OY ";
		const string p  = "P ";
		const string b  = "B ";
		const string t  = "T ";
		const string d  = "D ";
		
		const string k  = "K ";
		const string g  = "G ";
		const string f  = "F ";
		const string v  = "V ";
		const string TH = "TH ";
		
		const string DH = "DH ";
		const string s  = "S ";
		const string z  = "Z ";
		const string SH = "SH ";
		const string ZH = "ZH ";
		
		const string HH = "HH ";
		const string m  = "M ";
		const string n  = "N ";
		const string NG = "NG ";
		const string l  = "L ";
		
		const string w  = "W ";
		const string y  = "Y ";
		const string r  = "R ";
		const string CH = "CH ";
		const string j  = "JH ";
		
		const string WH = "W ";	// andersw: W used for WH in our "small phoneset" dictionary	

		//added by Rodrigo Cano on 2/26/2015. Not used for sythesis but can be used for start word generation.
		public static readonly ReadOnlyCollection<string> VOWEL_LIST = Array.AsReadOnly(new string[] {
		"AO", "AA", "IY", "UW", "EH", "IH", "UH", "AH", "AX", "AE", "EY", "AY", "OW", "AW", "OY", "ER"
		});
		
		public const int LEFT_PART = 0;
		public const int MATCH_PART = 1;
		public const int RIGHT_PART = 2;
		public const int OUT_PART = 3;
		
		
		/* = Punctuation */
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		
		static readonly string[][] punct_rules =
		{
			new string[] {Anything,      " ",            Anything,       Pause   },
			new string[] {Anything,      "-",            Anything,       Silent  },
			new string[] {".",           "'S",           Anything,       z     	},
			new string[] {"#:.E",        "'S",           Anything,       z     	},
			new string[] {"#",           "'S",           Anything,       z     	},
			new string[] {Anything,      "'",            Anything,       Silent  },
			new string[] {Anything,      ",",            Anything,       Pause   },
			new string[] {Anything,      ".",            Anything,       Pause   },
			new string[] {Anything,      "?",            Anything,       Pause   },
			new string[] {Anything,      "!",            Anything,       Pause   },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		
		static readonly string[][] A_rules =
		{
			new string[] {Anything,      "A",            Nothing,        AX    	},
			new string[] {Nothing,       "ARE",          Nothing,        AA+r  	},
			new string[] {Nothing,       "AR",           "O",            AX+r   	},
			new string[] {Anything,      "AR",           "#",            EH+r   	},
			new string[] {"^",           "AS",           "#",            EY+s   	},
			new string[] {Anything,      "A",            "WA",           AX    	},
			new string[] {Anything,      "AW",           Anything,       AO    	},
			new string[] {" :",          "ANY",          Anything,       EH+n+IY },
			new string[] {Anything,      "A",            "^+#",          EY    	},
			new string[] {"#:",          "ALLY",         Anything,       AX+l+IY },
			new string[] {Nothing,       "AL",           "#",            AX+l    },
			new string[] {Anything,      "AGAIN",        Anything,       AX+g+EH+n },
			new string[] {"#:",          "AG",           "E",            IH+j	},
			new string[] {Anything,      "A",            "^+:#",         AE	},
			new string[] {" :",          "A",            "^+ ",          EY      },
			new string[] {Anything,      "A",            "^%",           EY      },
			new string[] {Nothing,       "ARR",          Anything,       AX+r    },
			new string[] {Anything,      "ARR",          Anything,       AE+r    },
			new string[] {" :",          "AR",           Nothing,        AA+r    },
			new string[] {Anything,      "AR",           Nothing,        ER      },
			new string[] {Anything,      "AR",           Anything,       AA+r	},
			new string[] {Anything,      "AIR",          Anything,       EH+r	},
			new string[] {Anything,      "AI",           Anything,       EY      },
			new string[] {Anything,      "AY",           Anything,       EY      },
			new string[] {Anything,      "AU",           Anything,       AO      },
			new string[] {"#:",          "AL",           Nothing,        AX+l    },
			new string[] {"#:",          "ALS",          Nothing,        AX+l+z  },
			new string[] {Anything,      "ALK",          Anything,       AO+k    },
			new string[] {Anything,      "AL",           "^",            AO+l    },
			new string[] {" :",          "ABLE",         Anything,       EY+b+AX+l },
			new string[] {Anything,      "ABLE",         Anything,       AX+b+AX+l },
			new string[] {Anything,      "ANG",          "+",            EY+n+j  },
			new string[] {Anything,      "A",            Anything,       AE    },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		
		static readonly string[][] B_rules =
		{
			new string[] {Nothing,       "BE",           "^#",           b+IH    },
			new string[] {Anything,      "BEING",        Anything,       b+IY+IH+NG },
			new string[] {Nothing,       "BOTH",         Nothing,        b+OW+TH },
			new string[] {Nothing,       "BUS",          "#",            b+IH+z  },
			new string[] {Anything,      "BUIL",         Anything,       b+IH+l  },
			new string[] {Anything,      "B",            Anything,       b       },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  },
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		static readonly string[][] C_rules =
		{
			new string[] {Nothing,       "CH",           "^",            k       },
			new string[] {"^E",          "CH",           Anything,       k       },
			new string[] {Anything,      "CH",           Anything,       CH      },
			new string[] {" S",          "CI",           "#",            s+AY    },
			new string[] {Anything,      "CI",           "A",            SH      },
			new string[] {Anything,      "CI",           "O",            SH      },
			new string[] {Anything,      "CI",           "EN",           SH      },
			new string[] {Anything,      "C",            "+",            s	},
			new string[] {Anything,      "CK",           Anything,       k       },
			new string[] {Anything,      "COM",          "%",            k+AH+m  },
			new string[] {Anything,      "C",            Anything,       k       },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		
		static readonly string[][] D_rules =
		{
			new string[] {"#:",          "DED",          Nothing,        d+IH+d  },
			new string[] {".E",          "D",            Nothing,        d       },
			new string[] {"#:^E",        "D",            Nothing,        t     	},
			new string[] {Nothing,       "DE",           "^#",           d+IH   	},
			new string[] {Nothing,       "DO",           Nothing,        d+UW   	},
			new string[] {Nothing,       "DOES",         Anything,       d+AH+z  },
			new string[] {Nothing,       "DOING",        Anything,       d+UW+IH+NG },
			new string[] {Nothing,       "DOW",          Anything,       d+AW    },
			new string[] {Anything,      "DU",           "A",            j+UW    },
			new string[] {Anything,      "D",            Anything,       d       },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		
		static readonly string[][] E_rules =
		{
			new string[] {"#:",          "E",            Nothing,        Silent  },
			new string[] {"':^",         "E",            Nothing,        Silent  },
			new string[] {" :",          "E",            Nothing,        IY      },
			new string[] {"#",           "ED",           Nothing,        d       },
			new string[] {"#:",          "E",            "D ",           Silent  },
			new string[] {Anything,      "EV",           "ER",           EH+v    },
			new string[] {Anything,      "E",            "^%",           IY      },
			new string[] {Anything,      "ERI",          "#",            IY+r+IY },
			new string[] {Anything,      "ERI",          Anything,       EH+r+IH },
			new string[] {"#:",          "ER",           "#",            ER      },
			new string[] {Anything,      "ER",           "#",            EH+r    },
			new string[] {Anything,      "ER",           Anything,       ER      },
			new string[] {Nothing,       "EVEN",         Anything,       IY+v+EH+n },
			new string[] {"#:",          "E",            "W",            Silent  },
			new string[] {"T",           "EW",           Anything,       UW      },
			new string[] {"S",           "EW",           Anything,       UW      },
			new string[] {"R",           "EW",           Anything,       UW    	},
			new string[] {"D",           "EW",           Anything,       UW    	},
			new string[] {"L",           "EW",           Anything,       UW   	},
			new string[] {"Z",           "EW",           Anything,       UW      },
			new string[] {"N",           "EW",           Anything,       UW      },
			new string[] {"J",           "EW",           Anything,       UW      },
			new string[] {"TH",          "EW",           Anything,       UW      },
			new string[] {"CH",          "EW",           Anything,       UW      },
			new string[] {"SH",          "EW",           Anything,       UW      },
			new string[] {Anything,      "EW",           Anything,       y+UW    },
			new string[] {Anything,      "E",            "O",            IY      },
			new string[] {"#:S",         "ES",           Nothing,        IH+z    },
			new string[] {"#:C",         "ES",           Nothing,        IH+z    },
			new string[] {"#:G",         "ES",           Nothing,        IH+z    },
			new string[] {"#:Z",         "ES",           Nothing,        IH+z    },
			new string[] {"#:X",         "ES",           Nothing,        IH+z    },
			new string[] {"#:J",         "ES",           Nothing,        IH+z    },
			new string[] {"#:CH",        "ES",           Nothing,        IH+z    },
			new string[] {"#:SH",        "ES",           Nothing,        IH+z    },
			new string[] {"#:",          "E",            "S ",           Silent  },
			new string[] {"#:",          "ELY",          Nothing,        l+IY    },
			new string[] {"#:",          "EMENT",        Anything,       m+EH+n+t },
			new string[] {Anything,      "EFUL",         Anything,       f+UH+l  },
			new string[] {Anything,      "EE",           Anything,       IY      },
			new string[] {Anything,      "EARN",         Anything,       ER+n    },
			new string[] {Nothing,       "EAR",          "^",            ER      },
			new string[] {Anything,      "EAD",          Anything,       EH+d    },
			new string[] {"#:",          "EA",           Nothing,        IY+AX   },
			new string[] {Anything,      "EA",           "SU",           EH      },
			new string[] {Anything,      "EA",           Anything,       IY      },
			new string[] {Anything,      "EIGH",         Anything,       EY      },
			new string[] {Anything,      "EI",           Anything,       IY      },
			new string[] {Nothing,       "EYE",          Anything,       AY      },
			new string[] {Anything,      "EY",           Anything,       IY      },
			new string[] {Anything,      "EU",           Anything,       y+UW     },
			new string[] {Anything,      "E",            Anything,       EH      },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		
		static readonly string[][] F_rules =
		{
			new string[] {Anything,      "FUL",          Anything,       f+UH+l  },
			new string[] {Anything,      "F",            Anything,       f       },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		
		static readonly string[][] G_rules =
		{
			new string[] {Anything,      "GIV",          Anything,       g+IH+v  },
			new string[] {Nothing,       "G",            "I^",           g       },
			new string[] {Anything,      "GE",           "T",            g+EH     },
			new string[] {"SU",          "GGES",         Anything,       g+j+EH+s },
			new string[] {Anything,      "GG",           Anything,       g       },
			new string[] {" B#",         "G",            Anything,       g       },
			new string[] {Anything,      "G",            "+",            j	},
			new string[] {Anything,      "GREAT",        Anything,       g+r+EY+t },
			new string[] {"#",           "GH",           Anything,       Silent  },
			new string[] {Anything,      "G",            Anything,       g       },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		static readonly string[][] H_rules =
		{
			new string[] {Nothing,       "HAV",          Anything,       HH+AE+v  },
			new string[] {Nothing,       "HERE",         Anything,       HH+IY+r  },
			new string[] {Nothing,       "HOUR",         Anything,       AW+ER   },
			new string[] {Anything,      "HOW",          Anything,       HH+AW    },
			new string[] {Anything,      "H",            "#",            HH       },
			new string[] {Anything,      "H",            Anything,       Silent  },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		static readonly string[][] I_rules =
		{
			new string[] {Nothing,       "IN",           Anything,       IH+n    },
			new string[] {Nothing,       "I",            Nothing,        AY      },
			new string[] {Anything,      "IN",           "D",            AY+n    },
			new string[] {Anything,      "IER",          Anything,       IY+ER   },
			new string[] {"#:R",         "IED",          Anything,       IY+d    },
			new string[] {Anything,      "IED",          Nothing,        AY+d    },
			new string[] {Anything,      "IEN",          Anything,       IY+EH+n },
			new string[] {Anything,      "IE",           "T",            AY+EH   },
			new string[] {" :",          "I",            "%",            AY      },
			new string[] {Anything,      "I",            "%",            IY      },
			new string[] {Anything,      "IE",           Anything,       IY      },
			new string[] {Anything,      "I",            "^+:#",         IH      },
			new string[] {Anything,      "IR",           "#",            AY+r    },
			new string[] {Anything,      "IZ",           "%",            AY+z    },
			new string[] {Anything,      "IS",           "%",            AY+z    },
			new string[] {Anything,      "I",            "D%",           AY      },
			new string[] {"+^",          "I",            "^+",           IH      },
			new string[] {Anything,      "I",            "T%",           AY      },
			new string[] {"#:^",         "I",            "^+",           IH      },
			new string[] {Anything,      "I",            "^+",           AY      },
			new string[] {Anything,      "IR",           Anything,       ER      },
			new string[] {Anything,      "IGH",          Anything,       AY      },
			new string[] {Anything,      "ILD",          Anything,       AY+l+d  },
			new string[] {Anything,      "IGN",          Nothing,        AY+n    },
			new string[] {Anything,      "IGN",          "^",            AY+n    },
			new string[] {Anything,      "IGN",          "%",            AY+n    },
			new string[] {Anything,      "IQUE",         Anything,       IY+k    },
			new string[] {Anything,      "I",            Anything,       IH      },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		static readonly string[][] J_rules =
		{
			new string[] {Anything,      "J",            Anything,       j       },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		static readonly string[][] K_rules =
		{
			new string[] {Nothing,       "K",            "N",            Silent  },
			new string[] {Anything,      "K",            Anything,       k       },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		static readonly string[][] L_rules =
		{
			new string[] {Anything,      "LO",           "C#",           l+OW    },
			new string[] {"L",           "L",            Anything,       Silent  },
			new string[] {"#:^",         "L",            "%",            AX+l    },
			new string[] {Anything,      "LEAD",         Anything,       l+IY+d  },
			new string[] {Anything,      "L",            Anything,       l       },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		static readonly string[][] M_rules =
		{
			new string[] {Anything,      "MOV",          Anything,       m+UW+v  },
			new string[] {Anything,      "M",            Anything,       m       },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		static readonly string[][] N_rules =
		{
			new string[] {"E",           "NG",           "+",            n+j     },
			new string[] {Anything,      "NG",           "R",            NG+g    },
			new string[] {Anything,      "NG",           "#",            NG+g    },
			new string[] {Anything,      "NGL",          "%",            NG+g+AX+l },
			new string[] {Anything,      "NG",           Anything,       NG      },
			new string[] {Anything,      "NK",           Anything,       NG+k    },
			new string[] {Nothing,       "NOW",          Nothing,        n+AW    },
			new string[] {Anything,      "N",            Anything,       n       },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		static readonly string[][] O_rules =
		{
			new string[] {Anything,      "OF",           Nothing,        AX+v    },
			new string[] {Anything,      "OROUGH",       Anything,       ER+OW   },
			new string[] {"#:",          "OR",           Nothing,        ER      },
			new string[] {"#:",          "ORS",          Nothing,        ER+z    },
			new string[] {Anything,      "OR",           Anything,       AO+r    },
			new string[] {Nothing,       "ONE",          Anything,       w+AH+n  },
			new string[] {Anything,      "OW",           Anything,       OW      },
			new string[] {Nothing,       "OVER",         Anything,       OW+v+ER },
			new string[] {Anything,      "OV",           Anything,       AH+v    },
			new string[] {Anything,      "O",            "^%",           OW      },
			new string[] {Anything,      "O",            "^EN",          OW      },
			new string[] {Anything,      "O",            "^I#",          OW      },
			new string[] {Anything,      "OL",           "D",            OW+l    },
			new string[] {Anything,      "OUGHT",        Anything,       AO+t    },
			new string[] {Anything,      "OUGH",         Anything,       AH+f    },
			new string[] {Nothing,       "OU",           Anything,       AW      },
			new string[] {"H",           "OU",           "S#",           AW      },
			new string[] {Anything,      "OUS",          Anything,       AX+s    },
			new string[] {Anything,      "OUR",          Anything,       AO+r    },
			new string[] {Anything,      "OULD",         Anything,       UH+d    },
			new string[] {"^",           "OU",           "^L",           AH      },
			new string[] {Anything,      "OUP",          Anything,       UW+p    },
			new string[] {Anything,      "OU",           Anything,       AW      },
			new string[] {Anything,      "OY",           Anything,       OY      },
			new string[] {Anything,      "OING",         Anything,       OW+IH+NG },
			new string[] {Anything,      "OI",           Anything,       OY   	},
			new string[] {Anything,      "OOR",          Anything,       AO+r   	},
			new string[] {Anything,      "OOK",          Anything,       UH+k    },
			new string[] {Anything,      "OOD",          Anything,       UH+d    },
			new string[] {Anything,      "OO",           Anything,       UW      },
			new string[] {Anything,      "O",            "E",            OW      },
			new string[] {Anything,      "O",            Nothing,        OW      },
			new string[] {Anything,      "OA",           Anything,       OW      },
			new string[] {Nothing,       "ONLY",         Anything,       OW+n+l+IY },
			new string[] {Nothing,       "ONCE",         Anything,       w+AH+n+s },
			new string[] {Anything,      "ON'T",         Anything,       OW+n+t  },
			new string[] {"C",           "O",            "N",            AA    	},
			new string[] {Anything,      "O",            "NG",           AO      },
			new string[] {" :^",         "O",            "N",            AH      },
			new string[] {"I",           "ON",           Anything,       AX+n    },
			new string[] {"#:",          "ON",           Nothing,        AX+n    },
			new string[] {"#^",          "ON",           Anything,       AX+n    },
			new string[] {Anything,      "O",            "ST ",          OW      },
			new string[] {Anything,      "OF",           "^",            AO+f    },
			new string[] {Anything,      "OTHER",        Anything,       AH+DH+ER },
			new string[] {Anything,      "OSS",          Nothing,        AO+s    },
			new string[] {"#:^",         "OM",           Anything,       AH+m    },
			new string[] {Anything,      "O",            Anything,       AA      },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		static readonly string[][] P_rules =
		{
			new string[] {Anything,      "PH",           Anything,       f       },
			new string[] {Anything,      "PEOP",         Anything,       p+IY+p  },
			new string[] {Anything,      "POW",          Anything,       p+AW    },
			new string[] {Anything,      "PUT",          Nothing,        p+UH+t  },
			new string[] {Anything,      "P",            Anything,       p       },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		static readonly string[][] Q_rules =
		{
			new string[] {Anything,      "QUAR",         Anything,       k+w+AO+r },
			new string[] {Anything,      "QU",           Anything,       k+w      },
			new string[] {Anything,      "Q",            Anything,       k        },
			new string[] {Anything,      "!%@$#",        Anything,       Silent   }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		static readonly string[][] R_rules =
		{
			new string[] {Nothing,       "RE",           "^#",           r+IY    },
			new string[] {Anything,      "R",            Anything,       r       },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		static readonly string[][] S_rules =
		{
			new string[] {Anything,      "SH",           Anything,       SH      },
			new string[] {"#",           "SION",         Anything,       ZH+AX+n },
			new string[] {Anything,      "SOME",         Anything,       s+AH+m  },
			new string[] {"#",           "SUR",          "#",            ZH+ER   },
			new string[] {Anything,      "SUR",          "#",            SH+ER   },
			new string[] {"#",           "SU",           "#",            ZH+UW   },
			new string[] {"#",           "SSU",          "#",            SH+UW   },
			new string[] {"#",           "SED",          Nothing,        z+d     },
			new string[] {"#",           "S",            "#",            z       },
			new string[] {Anything,      "SAID",         Anything,       s+EH+d  },
			new string[] {"^",           "SION",         Anything,       SH+AX+n },
			new string[] {Anything,      "S",            "S",            Silent  },
			new string[] {".",           "S",            Nothing,        z       },
			new string[] {"#:.E",        "S",            Nothing,        z       },
			new string[] {"#:^##",       "S",            Nothing,        z       },
			new string[] {"#:^#",        "S",            Nothing,        s       },
			new string[] {"U",           "S",            Nothing,        s       },
			new string[] {" :#",         "S",            Nothing,        z       },
			new string[] {Nothing,       "SCH",          "#",            s+k     },
			new string[] {Nothing,       "SCH",          Anything,       SH      },
			new string[] {Anything,      "S",            "C+",           Silent  },
			new string[] {"#",           "SM",           Anything,       z+m     },
			new string[] {"#",           "SN",           "'",            z+AX+n  },
			new string[] {Anything,      "S",            Anything,       s       },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		static readonly string[][] T_rules =
		{
			new string[] {Nothing,       "THE",          Nothing,        DH+AX   },
			new string[] {Anything,      "TO",           Nothing,        t+UW    },
			new string[] {Anything,      "THAT",         Nothing,        DH+AE+t },
			new string[] {Nothing,       "THIS",         Nothing,        DH+IH+s },
			new string[] {Nothing,       "THEY",         Anything,       DH+EY   },
			new string[] {Nothing,       "THERE",        Anything,       DH+EH+r },
			new string[] {Anything,      "THER",         Anything,       DH+ER   },
			new string[] {Anything,      "THEIR",        Anything,       DH+EH+r },
			new string[] {Nothing,       "THAN",         Nothing,        DH+AE+n },
			new string[] {Nothing,       "THEM",         Nothing,        DH+EH+m },
			new string[] {Anything,      "THESE",        Nothing,        DH+IY+z },
			new string[] {Nothing,       "THEN",         Anything,       DH+EH+n },
			new string[] {Anything,      "THROUGH",      Anything,       TH+r+UW },
			new string[] {Anything,      "THOSE",        Anything,       DH+OW+z },
			new string[] {Anything,      "THOUGH",       Nothing,        DH+OW   },
			new string[] {Nothing,       "THUS",         Anything,       DH+AH+s },
			new string[] {Anything,      "TH",           Anything,       TH      },
			new string[] {"#:",          "TED",          Nothing,        t+IH+d  },
			new string[] {"S",           "TI",           "#N",           CH      },
			new string[] {Anything,      "TI",           "O",            SH      },
			new string[] {Anything,      "TI",           "A",            SH      },
			new string[] {Anything,      "TIEN",         Anything,       SH+AX+n },
			new string[] {Anything,      "TUR",          "#",            CH+ER   },
			new string[] {Anything,      "TU",           "A",            CH+UW   },
			new string[] {Nothing,       "TWO",          Anything,       t+UW    },
			new string[] {Anything,      "T",            Anything,       t       },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		static readonly string[][] U_rules =
		{
			new string[] {Nothing,       "UN",           "I",            y+UW+n  },
			new string[] {Nothing,       "UN",           Anything,       AH+n    },
			new string[] {Nothing,       "UPON",         Anything,       AX+p+AO+n },
			new string[] {"T",           "UR",           "#",            UH+r    },
			new string[] {"S",           "UR",           "#",            UH+r    },
			new string[] {"R",           "UR",           "#",            UH+r    },
			new string[] {"D",           "UR",           "#",            UH+r    },
			new string[] {"L",           "UR",           "#",            UH+r    },
			new string[] {"Z",           "UR",           "#",            UH+r    },
			new string[] {"N",           "UR",           "#",            UH+r    },
			new string[] {"J",           "UR",           "#",            UH+r    },
			new string[] {"TH",          "UR",           "#",            UH+r    },
			new string[] {"CH",          "UR",           "#",            UH+r    },
			new string[] {"SH",          "UR",           "#",            UH+r    },
			new string[] {Anything,      "UR",           "#",            y+UH+r  },
			new string[] {Anything,      "UR",           Anything,       ER      },
			new string[] {Anything,      "U",            "^ ",           AH      },
			new string[] {Anything,      "U",            "^^",           AH      },
			new string[] {Anything,      "UY",           Anything,       AY      },
			new string[] {" G",          "U",            "#",            Silent  },
			new string[] {"G",           "U",            "%",            Silent  },
			new string[] {"G",           "U",            "#",            w       },
			new string[] {"#N",          "U",            Anything,       y+UW    },
			new string[] {"T",           "U",            Anything,       UW      },
			new string[] {"S",           "U",            Anything,       UW      },
			new string[] {"R",           "U",            Anything,       UW      },
			new string[] {"D",           "U",            Anything,       UW      },
			new string[] {"L",           "U",            Anything,       UW      },
			new string[] {"Z",           "U",            Anything,       UW      },
			new string[] {"N",           "U",            Anything,       UW      },
			new string[] {"J",           "U",            Anything,       UW      },
			new string[] {"TH",          "U",            Anything,       UW      },
			new string[] {"CH",          "U",            Anything,       UW      },
			new string[] {"SH",          "U",            Anything,       UW      },
			new string[] {Anything,      "U",            Anything,       y+UW    },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		static readonly string[][] V_rules =
		{
			new string[] {Anything,      "VIEW",         Anything,       v+y+UW  },
			new string[] {Anything,      "V",            Anything,       v       },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		static readonly string[][] W_rules =
		{
			new string[] {Nothing,       "WERE",         Anything,       w+ER    },
			new string[] {Anything,      "WA",           "S",            w+AA    },
			new string[] {Anything,      "WA",           "T",            w+AA    },
			new string[] {Anything,      "WHERE",        Anything,       WH+EH+r },
			new string[] {Anything,      "WHAT",         Anything,       WH+AA+t },
			new string[] {Anything,      "WHOL",         Anything,       HH+OW+l  },
			new string[] {Anything,      "WHO",          Anything,       HH+UW    },
			new string[] {Anything,      "WH",           Anything,       WH      },
			new string[] {Anything,      "WAR",          Anything,       w+AO+r  },
			new string[] {Anything,      "WOR",          "^",            w+ER    },
			new string[] {Anything,      "WR",           Anything,       r       },
			new string[] {Anything,      "W",            Anything,       w       },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		static readonly string[][] X_rules =
		{
			new string[] {Anything,      "X",            Anything,       k+s     },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		static readonly string[][] Y_rules =
		{
			new string[] {Anything,      "YOUNG",        Anything,       y+AH+NG },
			new string[] {Nothing,       "YOU",          Anything,       y+UW    },
			new string[] {Nothing,       "YES",          Anything,       y+EH+s  },
			new string[] {Nothing,       "Y",            Anything,       y       },
			new string[] {"#:^",         "Y",            Nothing,        IY      },
			new string[] {"#:^",         "Y",            "I",            IY      },
			new string[] {" :",          "Y",            Nothing,        AY      },
			new string[] {" :",          "Y",            "#",            AY      },
			new string[] {" :",          "Y",            "^+:#",         IH      },
			new string[] {" :",          "Y",            "^#",           AY      },
			new string[] {Anything,      "Y",            Anything,       IH      },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		/*
**      LEFT_PART       MATCH_PART      RIGHT_PART      OUT_PART
*/
		static readonly string[][] Z_rules =
		{
			new string[] {Anything,      "Z",            Anything,       z       },
			new string[] {Anything,      "!%@$#",        Anything,       Silent  }
		};
		
		public static readonly string[][][] rules =
		{
			English.punct_rules,
			A_rules, B_rules, C_rules, D_rules, E_rules, F_rules, G_rules, 
			H_rules, I_rules, J_rules, K_rules, L_rules, M_rules, N_rules, 
			O_rules, P_rules, Q_rules, R_rules, S_rules, T_rules, U_rules, 
			V_rules, W_rules, X_rules, Y_rules, Z_rules
		};
	}

	public class Numbers
	{
		public const string ZERO = "Z IH R OW";
		public const string HUNDRED = "HH AH N D R AH D";
		public const string THOUSAND = "TH AW Z AH N D";
		public const string OH = "OW";//used as a connecting word as is 19-o' three

		public static readonly string[] UNITS =
		{
			"Z IH R OW", "W AH N", "T UW", "TH R IY", "F AO R", "F AY V", "S IH K S", "S EH V AH N", "EY T",
			"N AY N"
		};

		public static readonly string[] TEENS =
		{
			"T EH N", "IH L EH V AH N", "T W EH L V", "TH ER T IY N", "F AO R T IY N", "F IH F T IY N", 
			"S IH K S T IY N", "S EH V AH N T IY N", "EY T IY N", "N AY N T IY N"
		};

		public static readonly string[] TENS =
		{
			"ERROR", "T EH N", "T W EH N T IY", "TH ER D IY", "F AO R T IY", "F IH F T IY", "S IH K S T IY",
			"S EH V AH N T IY", "EY T IY", "N AY N T IY"
		};

//		Edited by Rodrigo Cano.
//		Used for testing
//		public const string ZERO = "zero";
//		public const string HUNDRED = "hundred";
//		public const string THOUSAND = "thousand";
//		public const string OH = "O'";//used as a connecting word as is 19-o' three
//		public const string AND = "and";//used as a connect word 100 hundred and three
//		
//		public static readonly string[] UNITS =
//		{
//			"zero", "one", "two", "three", "four", "five", "six", "seven", "eight",
//			"nine"
//		};
//		
//		public static readonly string[] TEENS =
//		{
//			"ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", 
//			"sixteen", "seventeen", "eighteen", "nineteen"
//		};
//		
//		public static readonly string[] TENS =
//		{
//			"ERROR", "ten", "twenty", "thirty", "forty", "fifty", "sixty",
//			"seventy", "eighty", "ninety"
//		};
	}

}