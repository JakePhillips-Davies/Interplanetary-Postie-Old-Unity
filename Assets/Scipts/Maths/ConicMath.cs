using System;
using UnityEngine;

namespace Orbits {

public struct ConicMath {

    public static double MeanAnomalyToTrueAnomaly(double meanAnomaly, double eccentricity, double trueAnomalyHint)
    {
        if (double.IsNaN(trueAnomalyHint)) {Debug.Log("True anomaly hint is NaN"); trueAnomalyHint = 0;}
        if (double.IsNaN(meanAnomaly)) {Debug.Log("Mean anomaly is NaN"); return trueAnomalyHint;}

        double tolerance = 0.000000001;
        int maxIter = 200;

        // Using Newton method to convert mean anomaly to true anomaly.
        if (eccentricity < 1) {

            meanAnomaly = Math.IEEERemainder(meanAnomaly, 2.0 * Math.PI);
            double eccentricAnomaly = 2.0 * Math.Atan(Math.Tan(0.5 * trueAnomalyHint) * Math.Sqrt((1.0 - eccentricity) / (1.0 + eccentricity)));

            double regionMin = meanAnomaly - eccentricity;
            double regionMax = meanAnomaly + eccentricity;

            for (int iter = 0; iter < maxIter; ++iter) // Newton iteration for Kepler equation;
            {
                eccentricAnomaly = Math.Clamp(eccentricAnomaly, regionMin, regionMax);

                double residual = eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly) - meanAnomaly;
                double derivative = 1.0 - eccentricity * Math.Cos(eccentricAnomaly);

                double delta = -residual / derivative;
                eccentricAnomaly += delta;
                if (Math.Abs(delta) < tolerance) { break; }

                if (iter + 1 == maxIter)
                { Debug.Log("Mean anomaly to true anomaly conversion failed: the solver did not converge. " + meanAnomaly +" "+ eccentricity +" "+ trueAnomalyHint); }
            }

            return 2.0 * Math.Atan(Math.Tan(0.5 * eccentricAnomaly) * Math.Sqrt((1.0 + eccentricity) / (1.0 - eccentricity)));

        } else if(eccentricity == 1) {

            double z = Math.Cbrt(3.0 * meanAnomaly + Math.Sqrt(1 + 9.0 * meanAnomaly * meanAnomaly));
            return 2.0 * Math.Atan(z - 1.0 / z);

        } else {

            double eccentricAnomaly = 2.0 * Math.Atanh(Math.Tan(0.5 * trueAnomalyHint) * Math.Sqrt((eccentricity - 1.0) / (eccentricity + 1.0)));
            for (int iter = 0; iter < maxIter; ++iter) // Newton iteration for Kepler equation;
            {
                double residual = eccentricity * Math.Sinh(eccentricAnomaly) - eccentricAnomaly - meanAnomaly;
                double derivative = eccentricity * Math.Cosh(eccentricAnomaly) - 1.0;

                double delta = -residual / derivative;
                eccentricAnomaly += delta;
                if (Math.Abs(delta) < tolerance) { break; }

                if (iter + 1 == maxIter)
                { Debug.Log("Mean anomaly to true anomaly conversion failed: the solver did not converge." + meanAnomaly +" "+ eccentricity +" "+ trueAnomalyHint); }
            }

            return 2.0 * Math.Atan(Math.Tanh(0.5 * eccentricAnomaly) * Math.Sqrt((eccentricity + 1.0) / (eccentricity - 1.0)));

        }
    }

    public static double TrueAnomalyToMeanAnomaly(double trueAnomaly, double eccentricity) {
        
        if (eccentricity < 1) {

            double eccentricAnomaly = 2.0 * Math.Atan(Math.Tan(0.5 * trueAnomaly) * Math.Sqrt((1.0 - eccentricity) / (1.0 + eccentricity)));
            return eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly);

        } else if(eccentricity == 1) {

            double tan = Math.Tan(0.5 * trueAnomaly);
            return 0.5 * tan * (1.0 + tan * tan / 3.0);

        } else {

            double eccentricAnomaly = 2.0 * Math.Atanh(Math.Tan(0.5 * trueAnomaly) * Math.Sqrt((eccentricity - 1.0) / (eccentricity + 1.0)));
            double meanAnomaly = eccentricity * Math.Sinh(eccentricAnomaly) - eccentricAnomaly;
            
            return meanAnomaly;

        }
    }
}

}