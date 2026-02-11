using System;

namespace cAlgo
{
    // Interface all moving average types must implement
    public interface MAInterface
    {
        // Calculate method returns a MAResult containing the calculated value(s)
        MAResult Calculate(int index);
        
        // Initialize method called when the MA is first created
        void Initialize();
    }
}
