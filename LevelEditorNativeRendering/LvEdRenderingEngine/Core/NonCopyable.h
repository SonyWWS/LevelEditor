//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

namespace LvEdEngine
{

class NonCopyable
  {
   protected:
      NonCopyable() {}
      ~NonCopyable() {}
   private:  
      NonCopyable( const NonCopyable& );
      NonCopyable& operator=( const NonCopyable& );
  };

}
