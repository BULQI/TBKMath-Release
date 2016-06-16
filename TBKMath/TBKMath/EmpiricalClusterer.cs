using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBKMath
{
    public class Ball<T>
    {
        private double radius;
        public double Radius { get { return radius; } }
        private T center;
        public T Center { get { return center; } }
        private List<T> elements;
        public List<T> Elements { get { return elements; } }
        public delegate double Metric(T element1, T element2);
        private Metric metric;
        private Dictionary<T, double> maxDistance;
        public T MostDistantElement;

        public Ball(Metric metric, List<T> initialElements)
        {
            this.metric = metric;
            elements = initialElements;
            maxDistance = new Dictionary<T, double>();
            foreach (T element1 in elements)
            {
                maxDistance.Add(element1, getMaxDistance(element1));
            }
            findCenter();
        }

        public Ball(Metric metric)
        {
            this.metric = metric;
            elements = new List<T>();
            maxDistance = new Dictionary<T, double>();
        }

        private double getMaxDistance(T element)
        {
            double maxDist = double.MinValue;
            foreach (T element2 in elements)
            {
                double dist = metric(element,element2);
                if (dist > maxDist)
                {
                    maxDist = dist;
                }
            }
            return maxDist;
        }

        public void AddElement(T element)
        {
            elements.Add(element);
            maxDistance.Clear();
            foreach (T element2 in elements)
            {
                maxDistance.Add(element2, getMaxDistance(element2));
            }
            // update center, radius
            findCenter(); // TODO: make more efficient
        }

        public void RemoveElement(T element)
        {
            elements.Remove(element);
            maxDistance.Clear();
            foreach (T element2 in elements)
            {
                maxDistance.Add(element2, getMaxDistance(element2));
            }
            findCenter();
        }

        private void findCenter()
        {
            radius = double.MaxValue;
            foreach (T member1 in elements)
            {
                if (maxDistance[member1] < radius)
                {
                    radius = maxDistance[member1];
                    center = member1;
                }
            }
            double greatestDistance = double.MinValue;
            foreach (T element in elements)
            {
                double dist = metric(element,center);
                if (dist > greatestDistance)
                {
                    greatestDistance = dist;
                    MostDistantElement = element;
                }
            }
        }

        private void updateAfterAddition(T element)
        {

        }

        private void updateAfterRemoval(T element)
        {

        }

        public static Tuple<List<Ball<T>>, bool, int> ClusterFixedNumberOfBalls(List<T> elements, Metric metric, int nBalls, int maxNumSteps)
        {
            if (elements.Count <= nBalls)
            {
                // nothing to do
                return new Tuple<List<Ball<T>>, bool, int>(null, false, 0);
            }

            List<Ball<T>> balls = new List<Ball<T>>();
            for (int i = 0; i < nBalls; i++)
            {
                balls.Add(new Ball<T>(metric));
            }
            Troschuetz.Random.Generators.MT19937Generator gen = new Troschuetz.Random.Generators.MT19937Generator() ;
            Troschuetz.Random.Distributions.Discrete.DiscreteUniformDistribution dud = new Troschuetz.Random.Distributions.Discrete.DiscreteUniformDistribution(gen);
            dud.Alpha = 0;
            dud.Beta = nBalls - 1;

            // start by assigning elements to balls at random
            Dictionary<T, Ball<T>> assigmnent = new Dictionary<T,Ball<T>>();
            foreach (T element in elements)
            {
                balls[dud.Next()].AddElement(element);
            }

            // check for empty balls
            for (int i = 0; i < nBalls; i++)
            {
                if (balls[i].Elements.Count == 0)
                {
                    // select another ball at random
                    bool done = false;
                    while (!done)
                    {
                        int iBall = dud.Next();
                        if (balls[iBall].Elements.Count > 1)
                        {
                            T elementToMove = balls[iBall].Elements[0];
                            balls[iBall].RemoveElement(elementToMove);
                            balls[i].AddElement(elementToMove);
                            done = true;
                        }
                    }
                }
            }

            foreach (Ball<T> ball in balls)
            {
                foreach (T element in ball.elements)
                {
                    assigmnent.Add(element, ball);
                }
            }

            Dictionary<T, double> distanceToCenter = new Dictionary<T, double>();
            double greatestRadius = double.MinValue;
            Ball<T> ballWithGreatestRadius = null;
            foreach (Ball<T> ball in balls)
            {
                if (ball.radius > greatestRadius)
                {
                    greatestRadius = ball.radius;
                    ballWithGreatestRadius = ball;
                }
            }

            int nStepsTaken = maxNumSteps;
            bool converged = false;
            for (int iStep = 0; iStep < maxNumSteps ; iStep++)
            {
                T outlier = ballWithGreatestRadius.MostDistantElement;
                double smallestDistance = double.MaxValue;
                Ball<T> nearestBall = null;

                foreach (Ball<T> ball in balls)
                {
                    double dist = metric(outlier,ball.Center);
                    if (dist < smallestDistance)
                    {
                        smallestDistance = dist;
                        nearestBall = ball;
                    }
                }

                // if the closest center is closer than current center, move it. Else end.
                if (smallestDistance < ballWithGreatestRadius.radius)
                {
                    // move outlier
                    ballWithGreatestRadius.RemoveElement(outlier);
                    nearestBall.AddElement(outlier);
                }
                else
                {
                    converged = true;
                    nStepsTaken = iStep;
                    break;
                }
                // find the next outlier
                greatestRadius = double.MinValue;
                foreach (Ball<T> ball in balls)
                {
                    if (ball.radius > greatestRadius)
                    {
                        greatestRadius = ball.radius;
                        ballWithGreatestRadius = ball;
                    }
                } 
            }
            return new Tuple<List<Ball<T>>, bool, int>(balls, converged, nStepsTaken);
        }

        public static Tuple<List<Ball<T>>, bool, int> Cluster2FixedNumberOfBalls(List<T> elements, Metric metric, int nBalls, int maxNumSteps)
        {
            if (elements.Count <= nBalls)
            {
                // nothing to do
                return new Tuple<List<Ball<T>>, bool, int>(null, false, 0);
            }

            List<Ball<T>> balls = new List<Ball<T>>() ;
            for (int i = 0; i < nBalls; i++)
            {
                balls.Add(new Ball<T>(metric));
            }
            Troschuetz.Random.Generators.MT19937Generator gen = new Troschuetz.Random.Generators.MT19937Generator();
            Troschuetz.Random.Distributions.Discrete.DiscreteUniformDistribution dud = new Troschuetz.Random.Distributions.Discrete.DiscreteUniformDistribution(gen);
            dud.Alpha = 0;
            dud.Beta = nBalls - 1;

            // start by assigning elements to balls at random
            Dictionary<T, Ball<T>> assigmnent = new Dictionary<T, Ball<T>>();
            foreach (T element in elements)
            {
                balls[dud.Next()].AddElement(element);
            }

            // check for empty balls
            for (int i = 0; i < nBalls; i++)
            {
                if (balls[i].Elements.Count == 0)
                {
                    // select another ball at random
                    bool done = false;
                    while (!done)
                    {
                        int iBall = dud.Next();
                        if (balls[iBall].Elements.Count > 1)
                        {
                            T elementToMove = balls[iBall].Elements[0];
                            balls[iBall].RemoveElement(elementToMove);
                            balls[i].AddElement(elementToMove);
                            done = true;
                        }
                    }
                }
            }

            foreach (Ball<T> ball in balls)
            {
                foreach (T element in ball.elements)
                {
                    assigmnent.Add(element, ball);
                }
            }

            int nStepsTaken = maxNumSteps;
            bool converged = false;
            for (int iStep = 0; iStep < maxNumSteps; iStep++)
            {
                int nChanged = 0;
                foreach (T element in elements)
                {
                    double smallestDistance = double.MaxValue;
                    Ball<T> nearestBall = null;
                    foreach (Ball<T> ball in balls)
                    {
                        double dist = metric(element, ball.Center);
                        if (dist < smallestDistance)
                        {
                            smallestDistance = dist;
                            nearestBall = ball;
                        }
                    }
                    if (nearestBall.Elements.Contains(element))
                        continue;

                    nearestBall.AddElement(element);
                    assigmnent[element].RemoveElement(element);
                    assigmnent.Remove(element);
                    assigmnent.Add(element, nearestBall);
                    nChanged++;
                }
                if (nChanged == 0)
                {
                    nStepsTaken = iStep;
                    converged = true;
                    break;
                }

            }
            return new Tuple<List<Ball<T>>, bool, int>(balls, converged, nStepsTaken);
        }

        public static Tuple<List<Ball<T>>, bool, int> Cluster2FixedNumberOfBalls(List<T>[] elements, Metric metric, int maxNumSteps)
        {
            int nBalls = elements.Length;
            List<Ball<T>> balls = new List<Ball<T>>();
            for (int i = 0; i < nBalls; i++)
            {
                if (elements[i].Count > 0)
                {
                    List<T> group = new List<T>(elements[i]);
                    balls.Add(new Ball<T>(metric, group));
                }
            }
            Dictionary<T, Ball<T>> assigmnent = new Dictionary<T, Ball<T>>();

            foreach (Ball<T> ball in balls)
            {
                foreach (T element in ball.elements)
                {
                    assigmnent.Add(element, ball);
                }
            }

            int nStepsTaken = maxNumSteps;
            bool converged = false;
            for (int iStep = 0; iStep < maxNumSteps; iStep++)
            {
                int nChanged = 0;
                foreach (List<T> group in elements)
                {
                    foreach (T element in group)
                    {
                        double smallestDistance = double.MaxValue;
                        Ball<T> nearestBall = null;
                        foreach (Ball<T> ball in balls)
                        {
                            double dist = metric(element, ball.Center);
                            if (dist < smallestDistance)
                            {
                                smallestDistance = dist;
                                nearestBall = ball;
                            }
                        }
                        if (nearestBall.Elements.Contains(element))
                            continue;

                        nearestBall.AddElement(element);
                        assigmnent[element].RemoveElement(element);
                        if (assigmnent[element].Elements.Count == 0)
                        {
                            balls.Remove(assigmnent[element]);
                        }
                        assigmnent.Remove(element);
                        assigmnent.Add(element, nearestBall);
                        nChanged++;
                    }
                }
                if (nChanged == 0)
                {
                    nStepsTaken = iStep;
                    converged = true;
                    break;
                }

            }
            return new Tuple<List<Ball<T>>, bool, int>(balls, converged, nStepsTaken);
        }


    }
}
