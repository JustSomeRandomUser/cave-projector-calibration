#include "BeamerKalibrierung.h"

using namespace gmtl;
using namespace vrj;
using namespace std;

BeamerKalibrierung::BeamerKalibrierung(Kernel* kern)
	: VRApp(kern),
	  __stepx(1.0f / (float)BEAMER_X_RES),
	  __stepy(1.0f / (float)BEAMER_Y_RES),
	  __saveMode(false),
	  __bigSteps(false),
	  __bigStepStretch(10.0f),
	  __activeNode(1),
	  __helperDistanceFilter(10),
	  __helperDirectionFilter(0.05f)
{
	// Fix für Testen zu Hause: Wenn nicht in der CAVE, so tun, als seien
	// wir das linke Auge des vorderen Screens
	if (!this->cave) {
		//__currentNode = 3;
		this->node = 1;
	}
	__wiiMote.addButtonObserver(*this);
	// Hier werden unsere Matrizen mit denen der berechnenden Rect-Klasse
	// "Verbunden". Dadurch werden bei jedem Frame automatisch die aktualisierten
	// Matrizen genutzt.
	for(size_t cnode = 0; cnode < NODE_COUNT; cnode ++){
		__helper[cnode].init(this->deviceCoordinates[cnode], this->bimberMatrix[cnode]);
	}
}

void BeamerKalibrierung::onButtonDown(const cave::VrjWiiMote::button_id_type id)
{
	int tmpNode = __activeNode -1;

	switch (id) {
	case cave::WII_LEFT:
		if(__bigSteps)
		{
			__helper[tmpNode].moveCorner(-__stepx * __bigStepStretch, 0);
		}
		else
		{
			__helper[tmpNode].moveCorner(-__stepx, 0);
		}
		break;
	case cave::WII_RIGHT:
		if(__bigSteps)
			__helper[tmpNode].moveCorner(__stepx * __bigStepStretch, 0);
		else
			__helper[tmpNode].moveCorner(__stepx, 0);
		break;
	case cave::WII_UP:
		if(__bigSteps)
			__helper[tmpNode].moveCorner(0, __stepy * __bigStepStretch);
		else
			__helper[tmpNode].moveCorner(0, __stepy);
		break;
	case cave::WII_DOWN:
		if(__bigSteps)
			__helper[tmpNode].moveCorner(0, -__stepy * __bigStepStretch);
		else
			__helper[tmpNode].moveCorner(0, -__stepy);
		break;
	case cave::WII_A:
		__helper[tmpNode].nextCorner();
		break;
	case cave::WII_B:
		__bigSteps=!__bigSteps;
		break;
	case cave::WII_MINUS:
		__saveMode = false;
		break;
	case cave::WII_PLUS:
		__saveMode = true;
		break;
	case cave::WII_1:
		__activeNode++;
		if(__activeNode > NODE_COUNT)
			__activeNode=1;
		break;
	case cave::WII_2:
		__activeNode--;
		if(__activeNode < 1)
			__activeNode=NODE_COUNT;
		break;
	default:
		break;
	}
}

// Ich hole mir hier Head- und Wand-Matrix, sowie zwei Buttons von der Wiimote 
// (Wand ist ein Eingabegeraet aehnlich der Wiimote, das frueher in VR-Umgebungen eingesetzt wurde)
void BeamerKalibrierung::preFrame()
{
	__wiiMote.update();
	__kinect.update();
}

void BeamerKalibrierung::__drawHelperText()
{
	const gmtl::Matrix44f& head = __kinect.getTracker(cave::KINECT_HEAD);
	const gmtl::Matrix44f& hand = __kinect.getTracker(cave::KINECT_HAND_RIGHT);
	const gmtl::Matrix44f& shoulder = __kinect.getTracker(cave::KINECT_SHOULDER_RIGHT);

	// Vektoren für Kopf, Hand und den Vektor zwischen Kopf und Hand
	gmtl::Vec3f vHead = gmtl::makeTrans<gmtl::Vec3f>(head);
	gmtl::Vec3f vHand = gmtl::makeTrans<gmtl::Vec3f>(hand);
	gmtl::Vec3f vShoulder = gmtl::makeTrans<gmtl::Vec3f>(shoulder);
	gmtl::Vec3f vHeadHand = vHand - vHead;
	gmtl::Vec3f vShoulderHand = vShoulder - vHand;

	gmtl::Vec3f vShoulderHandXZproj = vShoulderHand;
	vShoulderHandXZproj[1] = 0;
	// Aktivierung: Hand weit genug weg (auf XZ-Ebene) vom Kopf (Körpermitte)?
	if (__helperDistanceFilter.update(gmtl::length(vShoulderHandXZproj)) > 0.35f) {
		// Nun normalisieren (1m lang)
		gmtl::normalize(vHeadHand);
		vHeadHand = __helperDirectionFilter.update(vHeadHand);

		// Die Schrift 1.2m in Richtung der Hand vor dem Kopf anzeigen
		gmtl::Matrix44f tf = gmtl::makeTrans<gmtl::Matrix44f>(vHead + vHeadHand * 1.2f);
		// Die Schrift entsprechend drehen
		gmtl::setRot(tf, gmtl::Vec3f(0.0f, 0.0f, -1.0f), vHeadHand);

		glPushMatrix();
			glMultMatrixf(tf.getData());
			glDisable(GL_TEXTURE_2D);
			__helperTextPanel->drawAligned(__helperText);
		glPopMatrix();
	}
}

void BeamerKalibrierung::drawWall(float angle, gmtl::Vec3f rotation, bool active, int device) {
	
		
		glPushMatrix();
		glPushAttrib(GL_ENABLE_BIT);
			// Hier ist wohl das Model verdreht...
		glRotatef(angle, rotation[0], rotation[1], rotation[2]);
			glDisable(GL_LIGHTING);
			glEnable(GL_TEXTURE_2D);

			if(__saveMode && device == 1)
				model[device][SAVE_ON].draw();
			else
				model[device][SAVE_OFF].draw();

			// Es wird auf jedem Knoten nur die relevante Wand gezeichnet
			if(active) {
				// in contextInit werden die Models so geladen, dass von Corner
				// 1..4 immer Klein, Groß, ... im Array steht. bool -> int ist
				// implizit (false 0, true 1) laut C++-Standard
				model[device][__helper[device].getCorner() * 2 + __bigSteps].draw();
			} else {
				model[device][DISABLED].draw();
			}
		glPopAttrib();
	glPopMatrix();
}

void BeamerKalibrierung::draw()
{	
	VRApp::draw(increment);
	increment++;
	if(increment == 4)
		increment = 0;
	// Basisklasse aufrufen, damit die Bimbermatrix richtig gesetzt ist (Entzerrung)

	// Linke Wand
	__drawHelperText();
	drawWall(-90.0, gmtl::Vec3f(0.0, 1.0, 0.0), __activeNode == 1, 0);
	// Vordere Wand
	drawWall(-90.0,  gmtl::Vec3f(0.0, 1.0, 0.0), __activeNode == 2, 1);
	// Rechte Wand
	drawWall(-90.0,  gmtl::Vec3f(0.0, 1.0, 0.0), __activeNode == 3, 2);
	////// Hintere Wand projeziert als Boden
	drawWall(-90.0,  gmtl::Vec3f(0.0, 1.0, 0.0), __activeNode == 4, 3);
	//// linke wand
}

// wird einmalig pro OpenGL-Kontext aufgerufen
void BeamerKalibrierung::contextInit()
{
	// NICHT Basisklasse aufrufen, um die schwarzen Ränder nicht zu schwärzen
	initGLState();

	char objPath[16];
	char textureSmallPath[20];
	char textureBigPath[20];
	int cnode;
	char buf[16];

	// Ermittelt den den Fensternamen der aktuellen OpenGL Instanz
	vrj::GlDrawManager * gl_manager = dynamic_cast<vrj::GlDrawManager*>(this->getDrawManager());
	string name = gl_manager->currentUserData()->getGlWindow()->getDisplay()->getName();

	// Zeichnet nur die Relevante Wand in das Fenster
	if(name == "Leftscreen"){
		cnode = 0 ;
		sprintf_s(objPath, 16, "data/cave_%d.obj", 2);
		sprintf_s(buf, 16, "data/cave_a.png");
	}
	else if(name == "Frontscreen"){
		cnode = 1;
		sprintf_s(objPath, 16, "data/cave_%d.obj", 3);
		sprintf_s(buf, 16, "data/cave_b.png");
		model[cnode][SAVE_ON].loadFromFile("data/savemode.obj", "data/speichern-ein.png");
		model[cnode][SAVE_OFF].loadFromFile("data/savemode.obj", "data/speichern-aus.png");
	}
	else if(name == "Rightscreen"){
		cnode = 2;
		sprintf_s(objPath, 16, "data/cave_%d.obj", 5);
		sprintf_s(buf, 16, "data/cave_a.png");	
	}
	else if(name == "Floorscreen"){
		cnode = 3;
		sprintf_s(objPath, 16, "data/cave_%d.obj", 5);
		sprintf_s(buf, 16, "data/cave_c.png");
	}
	
	// Lädt die Disabled-Textur in das Model
	model[cnode][DISABLED].loadFromFile(objPath, buf);

	// Lädt die active-Texturen in das Model
	// insgesamt 8 Texturen(4 für Kleine,4 für die großen Ecken)
	for(int j = 1; j < 5; j++){
		sprintf_s(textureSmallPath, 20, "data/cave%02dk%d.png", cnode + 1, j);
		sprintf_s(textureBigPath, 20, "data/cave%02dg%d.png", cnode + 1, j);
		model[cnode][2 * (j - 1)].loadFromFile(objPath, textureSmallPath);
		model[cnode][2 * (j - 1) + 1].loadFromFile(objPath, textureBigPath);
	}
	
	// Zeichnet den Text der bei aktiver Kinect im Bild umher fliegt
	__helperTextPanel = new glhelper::GlPolygonText("C:\\windows\\fonts\\calibri.ttf", 0.2f, 3.0f, FTGL::ALIGN_CENTER);
	__helperText = L"-/|\\- AIOZXP8Y -/|\\-\n-/|\\- AIOX8Y -/|\\-\n-/|\\- AIX8 -/|\\-";
	__helperTextPanel->calcBBox(__helperText);
	__helperTextPanel->setMaxWidth(__helperTextPanel->calcedWidth() * 1.05f);
}

void BeamerKalibrierung::init()
{
	// Head, Wand etc initialisieren
	VRApp::init();
}

void BeamerKalibrierung::saveCalibrationFile()
{
	for(size_t cnode = 0; cnode < NODE_COUNT; cnode ++) {

		std::string filename = "cords/2.0/geraetekoordinaten"  + to_string(static_cast<long long>(cnode + 1)) + ".txt";
		printf("Schreibe Geraetekoordianten nach \"%s\"\n", filename);
		ofstream myfile;
		myfile.open (filename);

		vector<GLfloat> c = __helper[cnode].getBimberMatrixAndDeviceCoordinates();
		for (vector<GLfloat>::iterator iter=c.begin(); iter!=c.end(); iter++)
		{
			myfile << *iter << endl;
		}
		myfile.close();
	}
}

void BeamerKalibrierung::shutdown()
{
	if (__saveMode)
		this->saveCalibrationFile();
}

// Die Main-Funktion kann von jeder Anwendung angepasst werden, insofern passt sie auch ganz gut in
// die Datei zur Anwendungsklasse

#include <vrj/Kernel/Kernel.h>

int main(int argc, char* argv[])
{
	// Mindestens eine jconf-Datei muss immer uebergeben werden, im Simulatormodus z. B. standalone.jconf
	// in der CAVE z.B. wii_kinect.jconf
	if (argc <= 1)
	{
		std::cout << "\n\n";
		std::cout << "Usage: " << argv[0]
				<< " vjconfigfile[0] vjconfigfile[1] ... vjconfigfile[n]"
				<< std::endl;
		exit(1);
	}

	// Eine Singlton-Instanz des VR Juggler Kerns erzeugen
	Kernel* kernel = Kernel::instance();
	// Anwendungsobjekt instanziieren, hier eigenen Code einbauen 
	BeamerKalibrierung* application = new BeamerKalibrierung();

	// Alle config-Files laden
	for ( int i = 1; i < argc; ++i )
	{
		kernel->loadConfigFile(argv[i]);
	}

	// Den Kern starten
	kernel->start();

	// Dem Kern das Anwendungsobjekt zur Ausfuehrung uebergeben
	kernel->setApplication(application);
	printf("wait for kernel stop\n");
	// Auf die Beendigung des nebenlaeufig ausgefuehrten Kerns warten...
	kernel->waitForKernelStop();
	// Hier gelangt man in der CAVE nach Betaetigen des Home-Buttons der Wiimote hin,
	// jetzt koennen ggf. Daten gesichert werden

	// Applikation herunterfahren (Kalibrierung speichern, wenn Save Mode)
	application->shutdown();
	// Den vom Anwendungsobjekt belegten Speicher freigeben
	delete application;

	return 0;
}
