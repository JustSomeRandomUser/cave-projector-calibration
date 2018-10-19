#pragma once
template <typename T>
class vector3d {
public:
	T x, y, z;
    vector3d(T x = 0, T y = 0, T z = 0) :
        x(x), y(y), z(z)
    {}

    //T & operator()(T i, T j, T k) {
    //    return data[i*d2*d3 + j*d3 + k];
    //}

    //T const & operator()(T i, T j, T k) const {
    //    return data[i*d2*d3 + j*d3 + k];
    //}
};