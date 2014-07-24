
#ifndef _BLOODSTONE_UTILITIES_H_
#define _BLOODSTONE_UTILITIES_H_

class Utils
{
public:
    static bool LoadShaderResource(unsigned int id, std::string& content);
};

class Interpolator
{
public:
    Interpolator(float durationInSeconds);
    bool Update(float& fraction);

private:
    float GetSecondsSinceStarted(void) const;

    float mDurationInSeconds;
    float mCurrentTimeInSeconds;
    double mInversedFrequency;
    LARGE_INTEGER mStartTime;
};

#endif
