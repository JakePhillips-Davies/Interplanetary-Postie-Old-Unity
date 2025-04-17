using System;
using UnityEngine;

public struct ConicMath {

    public static double G = 6.6743015e-11;

    public static double true_anomaly_to_eccentric_anomaly(double true_anomaly, double eccentricity)
    {
        double beta = eccentricity / (1.0 + Math.Sqrt(1.0 - eccentricity * eccentricity));
        return true_anomaly - 2.0 * Math.Atan(beta * Math.Sin(true_anomaly) / (1.0 + beta * Math.Cos(true_anomaly)));
    }

    public static double eccentric_anomaly_to_true_anomaly(double eccentric_anomaly, double eccentricity)
    {
        double beta = eccentricity / (1.0 + Math.Sqrt(1.0 - eccentricity * eccentricity));
        return eccentric_anomaly + 2.0 * Math.Atan(beta * Math.Sin(eccentric_anomaly) / (1.0 - beta * Math.Cos(eccentric_anomaly)));
    }


    public static double mean_anomaly_to_true_anomaly(double mean_anomaly, double eccentricity, double true_anomaly_hint)
    {
        if (double.IsNaN(true_anomaly_hint)) {Debug.Log("True anomaly hint is NaN"); true_anomaly_hint = 0;}
        if (double.IsNaN(mean_anomaly)) {Debug.Log("Mean anomaly is NaN"); return true_anomaly_hint;}

        double tolerance = 0.000000001;
        int max_iter = 200;

        // Using Newton method to convert mean anomaly to true anomaly.
        if (eccentricity < 1) {

            mean_anomaly = Math.IEEERemainder(mean_anomaly, 2.0 * Math.PI);
            double eccentric_anomaly = 2.0 * Math.Atan(Math.Tan(0.5 * true_anomaly_hint) * Math.Sqrt((1.0 - eccentricity) / (1.0 + eccentricity)));

            double region_min = mean_anomaly - eccentricity;
            double region_max = mean_anomaly + eccentricity;

            for (int iter = 0; iter < max_iter; ++iter) // Newton iteration for Kepler equation;
            {
                eccentric_anomaly = Math.Clamp(eccentric_anomaly, region_min, region_max);

                double residual = eccentric_anomaly - eccentricity * Math.Sin(eccentric_anomaly) - mean_anomaly;
                double derivative = 1.0 - eccentricity * Math.Cos(eccentric_anomaly);

                double delta = -residual / derivative;
                eccentric_anomaly += delta;
                if (Math.Abs(delta) < tolerance) { break; }

                if (iter + 1 == max_iter)
                { Debug.Log("Mean anomaly to true anomaly conversion failed: the solver did not converge. " + mean_anomaly +" "+ eccentricity +" "+ true_anomaly_hint); }
            }

            return 2.0 * Math.Atan(Math.Tan(0.5 * eccentric_anomaly) * Math.Sqrt((1.0 + eccentricity) / (1.0 - eccentricity)));

        } else if(eccentricity == 1) {

            double z = Math.Cbrt(3.0 * mean_anomaly + Math.Sqrt(1 + 9.0 * mean_anomaly * mean_anomaly));
            return 2.0 * Math.Atan(z - 1.0 / z);

        } else {

            double eccentric_anomaly = 2.0 * Math.Atanh(Math.Tan(0.5 * true_anomaly_hint) * Math.Sqrt((eccentricity - 1.0) / (eccentricity + 1.0)));
            for (int iter = 0; iter < max_iter; ++iter) // Newton iteration for Kepler equation;
            {
                double residual = eccentricity * Math.Sinh(eccentric_anomaly) - eccentric_anomaly - mean_anomaly;
                double derivative = eccentricity * Math.Cosh(eccentric_anomaly) - 1.0;

                double delta = -residual / derivative;
                eccentric_anomaly += delta;
                if (Math.Abs(delta) < tolerance) { break; }

                if (iter + 1 == max_iter)
                { Debug.Log("Mean anomaly to true anomaly conversion failed: the solver did not converge." + mean_anomaly +" "+ eccentricity +" "+ true_anomaly_hint); }
            }

            return 2.0 * Math.Atan(Math.Tanh(0.5 * eccentric_anomaly) * Math.Sqrt((eccentricity + 1.0) / (eccentricity - 1.0)));

        }
    }

    public static double true_anomaly_to_mean_anomaly(double true_anomaly, double eccentricity)
    {
        //double eccentric_anomaly = true_anomaly_to_eccentric_anomaly(true_anomaly, eccentricity);
        //return eccentric_anomaly_to_mean_anomaly(eccentric_anomaly, eccentricity);
        
        if (eccentricity < 1) {

            double eccentric_anomaly = 2.0 * Math.Atan(Math.Tan(0.5 * true_anomaly) * Math.Sqrt((1.0 - eccentricity) / (1.0 + eccentricity)));
            return eccentric_anomaly - eccentricity * Math.Sin(eccentric_anomaly);

        } else if(eccentricity == 1) {

            double hta_tan = Math.Tan(0.5 * true_anomaly);
            return 0.5 * hta_tan * (1.0 + hta_tan * hta_tan / 3.0);

        } else {

            double eccentric_anomaly = 2.0 * Math.Atanh(Math.Tan(0.5 * true_anomaly) * Math.Sqrt((eccentricity - 1.0) / (eccentricity + 1.0)));
            double meanAnomaly = eccentricity * Math.Sinh(eccentric_anomaly) - eccentric_anomaly;
            
            return meanAnomaly;

        }
    }
}