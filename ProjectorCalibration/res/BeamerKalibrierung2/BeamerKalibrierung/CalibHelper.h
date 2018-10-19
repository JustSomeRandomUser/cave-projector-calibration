#pragma once

#include <fstream>
#include <sstream>
#include <iostream>
#include <vector>
#include <iterator>
#include <string>

#include <opencv/cv.h>
#include <Windows.h>
#include <GL/gl.h>

/** \brief Wird bei der Beamerkalibrierung als Helfer genutzt. */
class CalibHelper
{
public:
	/// Konstruktor: Speicherinitialisierung
	CalibHelper();
	/// Dekonstruktor: Speicher freigabe
	virtual ~CalibHelper();
	void init(GLfloat* deviceCoordinates, GLfloat* bimberMatrix);
	std::vector<GLfloat> getBimberMatrixAndDeviceCoordinates();
	void draw();
	void nextCorner();
	void previousCorner();
	int getCorner();
	void moveCorner(GLfloat x,GLfloat y);

private:
	void __calcBimberMatrix();
	int __corner;

	GLfloat* __deviceCoordinates;

	GLfloat* __bimberMatrix;
	GLfloat __tmp[16];
	GLfloat __tmp1, __tmp2;
	// OpenCV
	CvMat* __src_points;
	CvMat* __dst_points;
	CvMat* __homography;
};
