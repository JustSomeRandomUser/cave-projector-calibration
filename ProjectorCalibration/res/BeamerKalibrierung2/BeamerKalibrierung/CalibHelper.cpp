#include "CalibHelper.h"

CalibHelper::CalibHelper()
	: __corner(0)
{
	__src_points = cvCreateMat(4,2,CV_32FC1);
	__dst_points = cvCreateMat(4,2,CV_32FC1);
	__homography = cvCreateMat(3,3,CV_32FC1);
}

CalibHelper::~CalibHelper()
{
	cvReleaseMat(&__src_points);
	cvReleaseMat(&__dst_points);
	cvReleaseMat(&__dst_points);
}

void CalibHelper::init(GLfloat* deviceCoordinates, GLfloat* bimberMatrix)
{
	__bimberMatrix = bimberMatrix;
	__deviceCoordinates = deviceCoordinates;
}

std::vector<GLfloat> CalibHelper::getBimberMatrixAndDeviceCoordinates()
{
	std::vector<GLfloat> bMdC;
	for (int i = 0; i < 16; i++)
	{
		bMdC.push_back(__bimberMatrix[i]);
	}
	for (int i = 0; i < 8; i++)
	{
		bMdC.push_back(__deviceCoordinates[i]);
	}
	return bMdC;
}

void CalibHelper::draw()
{
	glPolygonMode(GL_FRONT_AND_BACK,GL_LINE);
	glBegin (GL_POLYGON);
	glVertex3f (__deviceCoordinates[0], __deviceCoordinates[1], -1);
	glVertex3f (__deviceCoordinates[2], __deviceCoordinates[3], -1);
	glVertex3f (__deviceCoordinates[4], __deviceCoordinates[5], -1);
	glVertex3f (__deviceCoordinates[6], __deviceCoordinates[7], -1);
	glEnd ();
	glPolygonMode(GL_FRONT_AND_BACK,GL_FILL);
}

void CalibHelper::nextCorner()
{
	__corner += 1;
	if(__corner > 3)
		__corner = 0;
}

void CalibHelper::previousCorner()
{
	__corner -= 1;
	if(__corner < 0)
		__corner= 3;
}

int CalibHelper::getCorner()
{
	return __corner;
}

void CalibHelper::moveCorner(GLfloat x, GLfloat y)
{
	__deviceCoordinates[__corner * 2] += x;
	__deviceCoordinates[__corner * 2 + 1] += y;
	__calcBimberMatrix();
}

void CalibHelper::__calcBimberMatrix()
{
	CV_MAT_ELEM( *__src_points, float, 0, 0) = -1;
	CV_MAT_ELEM( *__src_points, float, 0, 1) = 1;

	CV_MAT_ELEM( *__src_points, float, 1, 0) = 1;
	CV_MAT_ELEM( *__src_points, float, 1, 1) = 1;

	CV_MAT_ELEM( *__src_points, float, 2, 0) = 1;
	CV_MAT_ELEM( *__src_points, float, 2, 1) = -1;

	CV_MAT_ELEM( *__src_points, float, 3, 0) = -1;
	CV_MAT_ELEM( *__src_points, float, 3, 1) = -1;


	CV_MAT_ELEM( *__dst_points, float, 0, 0) = __deviceCoordinates[0];
	CV_MAT_ELEM( *__dst_points, float, 0, 1) = __deviceCoordinates[1];

	CV_MAT_ELEM( *__dst_points, float, 1, 0) = __deviceCoordinates[2];
	CV_MAT_ELEM( *__dst_points, float, 1, 1) = __deviceCoordinates[3];

	CV_MAT_ELEM( *__dst_points, float, 2, 0) = __deviceCoordinates[4];
	CV_MAT_ELEM( *__dst_points, float, 2, 1) = __deviceCoordinates[5];

	CV_MAT_ELEM( *__dst_points, float, 3, 0) = __deviceCoordinates[6];
	CV_MAT_ELEM( *__dst_points, float, 3, 1) = __deviceCoordinates[7];


	cvFindHomography(__src_points,__dst_points, __homography);

	__tmp[0] = CV_MAT_ELEM( *__homography, float, 0, 0);
	__tmp[1] = CV_MAT_ELEM( *__homography, float, 1, 0);
	__tmp[2] = CV_MAT_ELEM( *__homography, float, 2, 0);
	__tmp[3] = CV_MAT_ELEM( *__homography, float, 0, 1);
	__tmp[4] = CV_MAT_ELEM( *__homography, float, 1, 1);
	__tmp[5] = CV_MAT_ELEM( *__homography, float, 2, 1);
	__tmp[6] = CV_MAT_ELEM( *__homography, float, 0, 2);
	__tmp[7] = CV_MAT_ELEM( *__homography, float, 1, 2);
	__tmp[8] = CV_MAT_ELEM( *__homography, float, 2, 2);

	__tmp[0] = __tmp[0]/__tmp[8];
	__tmp[1] = __tmp[1]/__tmp[8];
	__tmp[2] = __tmp[2]/__tmp[8];
	__tmp[3] = __tmp[3]/__tmp[8];
	__tmp[4] = __tmp[4]/__tmp[8];
	__tmp[5] = __tmp[5]/__tmp[8];
	__tmp[6] = __tmp[6]/__tmp[8];
	__tmp[7] = __tmp[7]/__tmp[8];
	__tmp[8] = __tmp[8]/__tmp[8];

	if (__tmp[2]<0) {__tmp1=-__tmp[2];} else __tmp1=__tmp[2];
	if (__tmp[5]<0) {__tmp2=-__tmp[5];} else __tmp2=__tmp[5];
	__tmp[10] = 1-__tmp1-__tmp2;
	__bimberMatrix[0] = __tmp[0]; __bimberMatrix[4] = __tmp[3]; __bimberMatrix[8] = 0;         __bimberMatrix[12] = __tmp[6];
	__bimberMatrix[1] = __tmp[1]; __bimberMatrix[5] = __tmp[4]; __bimberMatrix[9] = 0;         __bimberMatrix[13] = __tmp[7];
	__bimberMatrix[2] = 0;        __bimberMatrix[6] = 0;        __bimberMatrix[10]= __tmp[10]; __bimberMatrix[14] = 0;
	__bimberMatrix[3] = __tmp[2]; __bimberMatrix[7] = __tmp[5]; __bimberMatrix[11]= 0;         __bimberMatrix[15] = 1;
}
