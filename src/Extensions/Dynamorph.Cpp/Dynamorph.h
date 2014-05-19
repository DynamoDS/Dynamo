// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the DYNAMORPH_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// DYNAMORPH_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef DYNAMORPH_EXPORTS
#define DYNAMORPH_API __declspec(dllexport)
#else
#define DYNAMORPH_API __declspec(dllimport)
#endif

// This class is exported from the Dynamorph.dll
class DYNAMORPH_API CDynamorph {
public:
	CDynamorph(void);
	// TODO: add your methods here.
};

extern DYNAMORPH_API int nDynamorph;

DYNAMORPH_API int fnDynamorph(void);
