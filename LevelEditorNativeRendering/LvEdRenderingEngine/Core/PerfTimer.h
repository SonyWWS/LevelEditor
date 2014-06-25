//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#include <assert.h>
#include <stdio.h>
#include <string>
#include "NonCopyable.h"

// -------------------------------------------------------------------------
namespace LvEdEngine
{

class PerfTimer : public NonCopyable
{
public:
    PerfTimer() : m_lastMs(0)
    {
        LARGE_INTEGER freq;
        QueryPerformanceFrequency(&freq);        
        m_freq = (double)freq.QuadPart;
    }
    void Start()
    {
        QueryPerformanceCounter(&m_start);
    }
    void Stop()
    {        
        LARGE_INTEGER stopCycles;
        QueryPerformanceCounter(&stopCycles);        
        double secondes = ( (double)(stopCycles.QuadPart - m_start.QuadPart) / m_freq );
        m_lastMs = secondes * 1000.0;
    }
    double ElapsedTimeMS()
    {
        return m_lastMs;
    }
    unsigned int ElapsedMilliseconds()
    {
        return (unsigned int)m_lastMs;
    }

private:
    double          m_freq;
    LARGE_INTEGER   m_start;
    double          m_lastMs;
};

// -------------------------------------------------------------------------------------------------------------
class ScopedTimer : public NonCopyable
{
public:
    ScopedTimer(const char* message)
    {
        m_message = message;
        m_timer.Start();
    }
    ~ScopedTimer()
    {
        m_timer.Stop();
        printf("%d ms - %s\n", m_timer.ElapsedMilliseconds(), m_message);
    }

private:
    ScopedTimer(){};
    PerfTimer m_timer;
    const char * m_message;

};


}; //namespace

