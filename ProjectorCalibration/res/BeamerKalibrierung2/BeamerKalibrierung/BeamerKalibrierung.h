// Applikation zum Testen der Kalibrierung der CAVE
// Diese Klasse dient auch dazu, zu demonstrieren, wie eine CAVE-Applikation entwickelt werden kann.
// Diese Klasse kann zu einer eigenen App (am besten mit anderem Namen!) geaendert werden.
// Thomas Jung, April 2012

#ifndef _BeamerKalibrierung
#define _BeamerKalibrierung

#ifdef _DEBUG
	#pragma comment (lib, "../Debug/CaveApi.lib")
	#pragma comment (lib, "opencv_core246d.lib")
	#pragma comment (lib, "opencv_calib3d246d.lib")
#else
	#pragma comment (lib, "../Release/CaveApi.lib")
	#pragma comment (lib, "opencv_core246.lib")
	#pragma comment (lib, "opencv_calib3d246.lib")
#endif

#include <vrj/Display/DisplayManager.h>
#include <vrj/Display/Display.h>

#include <gmtl/gmtl.h>
#include <glhelper/GlModel.h>
#include <glhelper/GlText.h>
#include <input/VrjWiiMote.h>
#include <input/VrjKinect.h>
#include <Filters.h>
#include <vrj\Draw\OGL\GlDrawManager.h>
#include <vrj\Draw\OGL\GlWindow.h>

#include "vector3d.h"

#include "VRApp.h"
#include "CaveUtil.h"
#include "CalibHelper.h"

#ifndef CAVE_WITH_GLTEXT
#error Für diese Anwendung muss muss CAVE_WITH_GLTEXT in cave_common.h \
einkommentiert werden!
#endif

// Jede Anwendungsklasse fuer die CAVE der HTW muss sich von VRApp ableiten
class BeamerKalibrierung : public VRApp, public cave::VrjWiiMote::button_observer_type
{
public:
	static const int BEAMER_X_RES = 1024;
	static const int BEAMER_Y_RES = 768;
	//BeamerKalibrierung(int i, Kernel* kern=NULL);
	BeamerKalibrierung(Kernel* kern=NULL);
	virtual ~BeamerKalibrierung () {;}
	virtual void init();
	virtual void shutdown();

	// brauchen wir nicht
	virtual void apiInit() {;}

	// Ueberladen wir
	virtual void contextInit();
	virtual void preFrame();
	virtual void draw();

	// brauchen wir nicht
	virtual void intraFrame() {;}
	virtual void postFrame() {;}

	void saveCalibrationFile();

	virtual void onButtonDown(const cave::VrjWiiMote::button_id_type id);
private:
	//int __currentNode; 
	// Aktiver Node (Beamer) von 1...8 - wie bei geraetekoordinaten[NODE].txt
	int __activeNode;
	// Speichern bei Verlassen?
	bool __saveMode;
	// Große/kleine Sprünge?
	bool __bigSteps;
	// Models: 2*4 Ecken, 1* Deaktiviert, 2*Savemode
	glhelper::GlModel model[NODE_COUNT][11];
	// Logik für die Kalibrierung
	CalibHelper __helper[NODE_COUNT+1];
	// Schritt in x-Richtung (klein)
	const float __stepx;
	// Schritt in y-Richtung (klein)
	const float __stepy;
	// Wie viel größer als der kleine soll der große Schritt sein?
	const float __bigStepStretch;

	cave::VrjWiiMote __wiiMote;
	cave::VrjKinect __kinect;

	// Hilfstext zum Checken der Kalibrierung
	glhelper::AbstractGlText* __helperTextPanel;
	wchar_t* __helperText;
	cavefilter::MovingAverage<float, true> __helperDistanceFilter;
	cavefilter::BrownDES<gmtl::Vec3f> __helperDirectionFilter;

	
	// Konstanten für das Model-Array
	// ACHTUNG: Bis 7 wird von den Corner-Texturen belegt.
	static const int DISABLED = 8;
	static const int SAVE_ON  = 9;
	static const int SAVE_OFF = 10;

	// Den Hilfstext zum Checken der Kalibrierung zeichnen, wenn man die Hand
	// ausstreckt. Wird dann vom Kopfpunkt über die Hand 1.2m verlängert.
	void __drawHelperText();

	// Zeichnet die Wand in das geöffnete Fenster
	void drawWall(float, gmtl::Vec3f, bool, int);
};


#endif
