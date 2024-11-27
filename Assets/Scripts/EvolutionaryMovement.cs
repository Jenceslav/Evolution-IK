using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class EvolutionaryMovement : MonoBehaviour
{
    public GameObject DefaultSystem; // prefab only
    public MovementSystem RootSystem;
    public List<MovementSystem> Systems = new List<MovementSystem>();
    public Population population;
    public float stepTime = 1;
    public int populationSize;
    public int Iterations;
    public int spacing;
    public bool useColoring;
    public bool runFast;

    private void Start()
    {
        RootSystem = new MovementSystem(DefaultSystem);


        List<float> goalValues = new List<float>();
        for (int i = 0; i < RootSystem.components2.Length; i++)
        {
            Vector3 goalPos = RootSystem.components2[i].GetEndPosition();

            goalValues.Add(goalPos.x);
            goalValues.Add(goalPos.y);
            goalValues.Add(goalPos.z);
        }

        List<bool> locks = new List<bool>();
        for (int i = 0; i < RootSystem.components.Length; i++)
        {
            locks.AddRange(RootSystem.components[i].GetLocks());
        }


        population = new Population(populationSize, RootSystem.components.Length, goalValues, locks, this, DefaultSystem);
        if (runFast)
        {
            RunFast();
        }
        else
        {
            StartCoroutine(Run());
        }
      
    }

    public MovementSystem CreateNewSystem(int i)
    {
        GameObject newSystemObject = Instantiate(DefaultSystem);
        newSystemObject.transform.parent = this.transform;
        newSystemObject.transform.localPosition = new Vector3(spacing * (i+1), 0, 0);
        MovementSystem movementSystem = new MovementSystem(newSystemObject);
        Systems.Add(movementSystem);
        movementSystem.offset = new Vector3(spacing * (i + 1), 0, 0);
        return movementSystem;
    }

    /// <summary>
    /// Runs the algorithm without waiting
    /// </summary>
    public void RunFast()
    {
       long t1 =  DateTime.Now.Ticks;


       for (int generation = 0; generation < Iterations; generation++)
       {
           // Evolutionary step
           population.Evolve(generation);
       }
       Individual bestIndividual = population.GetBestIndividual();
       List<float> newChromosome = bestIndividual.chromosome;


       Debug.Log($"Generation {Iterations}: Best fitness = {population.GetBestIndividual().fitness}");

       // interpolate for smooth transition
       List<float> current = new List<float>();
       for (int j = 0; j < newChromosome.Count; j++)
       {
           current.Add(newChromosome[j]);
       }
       for (int j = 0; j < newChromosome.Count / 3; j++)
       {
           List<float> extractedRot = current.GetRange(0, 3);
           current.RemoveRange(0, 3);
           RootSystem.components[j].SetFloats(extractedRot);
       } 


       long t2 = (DateTime.Now.Ticks - t1);
       long milliseconds = t2 / TimeSpan.TicksPerMillisecond; // Convert ticks to milliseconds

       Debug.Log($"Elapsed Time: {milliseconds} ms");
    }


    /// <summary>
    /// Runs the evolutionary algorithm for the specified number of generations. Has time waiting between frames of evolution for showcasing the results of given step.
    /// </summary>
    public IEnumerator Run()
    {
        for (int generation = 0; generation < Iterations; generation++)
        {
            // Evolutionary step
            population.Evolve(generation);
            Individual bestIndividual = population.GetBestIndividual();
            List<float> newChromosome = bestIndividual.chromosome;


            Debug.Log($"Generation {generation + 1}: Best fitness = {population.GetBestIndividual().fitness}");

                // interpolate for smooth transition
                List<float> current = new List<float>();
                for (int j = 0; j < newChromosome.Count; j++)
                {
                    current.Add(newChromosome[j]);
                }
                for (int j = 0; j < newChromosome.Count/3; j++)
                {
                    List<float> extractedRot = current.GetRange(0, 3);
                    current.RemoveRange(0, 3);
                    RootSystem.components[j].SetFloats(extractedRot);
                }
            

            yield return new WaitForSeconds(stepTime);
        }
    }


}

/// <summary>
/// Holds information about system - its components, rootObject, offset.
/// </summary>
public class MovementSystem
{
    public MovementComponent[] components;
    public EndStateComponent[] components2;
    public GameObject rootGameObject;
    public Vector3 offset;

    public MovementSystem(GameObject rootGameObject)
    {
        this.rootGameObject = rootGameObject;
        components = rootGameObject.GetComponentsInChildren<MovementComponent>();
        components2 = rootGameObject.GetComponentsInChildren<EndStateComponent>();
    }

    
    public List<float> GetPositionsAfterRotations(List<float> chromosome)
    {
        List<float> chromosome2 = new List<float>();
        for (int i = 0; i < chromosome.Count; i++)
        {
            chromosome2.Add(chromosome[i]);
        }

        for (int i = 0; i < components.Length; i++)
        {
            List<float> extractedRot = chromosome2.GetRange(0, 3);
            chromosome2.RemoveRange(0, 3);
            components[i].SetFloats(extractedRot);
        }

        List<float> positions = new List<float>();
        for (int i = 0; i < components2.Length; i++)
        {
            Vector3 pos = components2[i].GetCurrentPosition();
            pos -= offset;
            positions.Add(pos.x);
            positions.Add(pos.y);
            positions.Add(pos.z);
        }

        return positions;
    }
}

/// <summary>
/// Holds information about individual, chromosome, last calc fitness, system it belongs to.
/// </summary>
public class Individual
{
    public List<float> chromosome;
    public float fitness;
    public MovementSystem system;
    public Individual(List<float> chromosome, float fitness, MovementSystem system)
    {
        this.chromosome = chromosome;
        this.fitness = fitness;
        this.system = system;
    }
}

/// <summary>
/// Holds information about population (as a step of evolotionary alg.). Has methods for evolving - Mutation,Crossover,Combination,Init.
/// </summary>
public class Population 
{
    public EvolutionaryMovement evolutionaryMovement;
    public List<bool> locks;
    public List<float> movementGoal;
    public int populationSize;

    public List<Individual> population;
    
    public Population(int PopulationSize,int components1Length, List<float> movementGoal,List<bool> locks, EvolutionaryMovement evolutionaryMovement, GameObject system)
    {
        this.populationSize = PopulationSize;
        this.evolutionaryMovement = evolutionaryMovement;
        this.locks = locks;
        this.movementGoal = movementGoal;


        population = new List<Individual>();
        for (int i = 0; i < PopulationSize; i++)
        {
            MovementSystem movementSystem = this.evolutionaryMovement.CreateNewSystem(i);

            var chromosome = new List<float>();
            for (int j = 0; j < components1Length*3; j+=3)
            {
                List<float> rot = movementSystem.components[j / 3].GetCurrentFloats();
                chromosome.Add(rot[0]);
                chromosome.Add(rot[1]);
                chromosome.Add(rot[2]);
            }
            for (int y = 0; y < components1Length*3; y++)
            {
                if (locks[y] == false)
                {
                    chromosome[y] = (Random.Range(0f, 360f));
                }
            }


            population.Add(new Individual(chromosome, CostFunction(movementSystem.GetPositionsAfterRotations(chromosome), movementGoal, CostFunctionType.MSE),movementSystem));
        }
    }


    /// <summary>
    /// Selects two parents using tournament selection.
    /// </summary>
    private Individual SelectParent()
    {
        return population[Random.Range(0, population.Count)];
    }

    /// <summary>
    /// Performs single-point crossover between two parents.
    /// </summary>
    private float crossoverRate = 0.25f;

    private float combinationRate = 0.1f;
    private List<float> Crossover(List<float> parent1, List<float> parent2)
    {
        if (UnityEngine.Random.value > crossoverRate)
            return new List<float>(parent1); // No crossover; return a copy of parent1

        if (UnityEngine.Random.value > combinationRate)
        {
            var childComb = new List<float>();
            for (int i = 0; i < parent1.Count; i++)
            {
                childComb.Add((parent1[i]+parent2[i])/2f);
            }

            return childComb;
        }

        var child = new List<float>();
        for (int i = 0; i < parent1.Count; i++)
        {
            if (UnityEngine.Random.value < 50)
            {
                child.Add(parent1[i]);
            }
            else
            {
                child.Add(parent2[i]);
            }
        }
        return child;
    }

    /// <summary>
    /// Mutates a chromosome by slightly varying it in the direction of the goal.
    /// </summary>
    private float multipliedVariataionMax = 0.2f;
    private float addVariataionMax = 1.5f;
    private float mutationRate = 0.25f;
    private void Mutate(List<float> chromosome, int iteration)
    {
        for (int i = 0; i < chromosome.Count; i++)
        {
            if (locks[i] == false &&  UnityEngine.Random.value < mutationRate)
            {
                // Add random variation for exploration
                float variation1 = UnityEngine.Random.Range(-multipliedVariataionMax * chromosome[i], multipliedVariataionMax * chromosome[i]);
                float variation2 = UnityEngine.Random.Range(addVariataionMax, -addVariataionMax);  
                // Combine direction and variation for mutation
                chromosome[i] += variation1 + variation2; 
                chromosome[i] %= 360;
                
            }
        }
    }

    /// <summary>
    /// Evolves the population for one generation.
    /// </summary>
    public void Evolve(int generation)
    {
        // Evaluate fitness for all individuals
        foreach (var individual in population)
        {
            individual.fitness = CostFunction(individual.system.GetPositionsAfterRotations(individual.chromosome), movementGoal,CostFunctionType.MSE);
        }

        // Create a new population
        var newPopulation = new List<Individual>();
        population = new List<Individual>(population.OrderBy(ind => ind.fitness).ToArray());
        int i = 1;
        while (newPopulation.Count < populationSize)
        {
            if (i == 1)
            {
                newPopulation.Add(population[0]);
                i++;
                continue;
            }

            // Select parents
            var parent1 = SelectParent();
            var parent2 = SelectParent();

            // Crossover
            var childChromosome = Crossover(parent1.chromosome, parent2.chromosome);

            // Mutation
            Mutate(childChromosome,generation+1);

            // Add child to new population
            newPopulation.Add(new Individual(childChromosome, 0, evolutionaryMovement.Systems[populationSize-i] ));
            

            i++;
        }

        // Replace old population with the new one
        population = newPopulation;
        // Evaluate fitness for all individuals


        foreach (var individual in population)
        {
            individual.fitness = CostFunction(individual.system.GetPositionsAfterRotations(individual.chromosome), movementGoal, CostFunctionType.MSE);
        }
        if (evolutionaryMovement.useColoring)
        {
            population = new List<Individual>(population.OrderBy(ind => ind.fitness).ToArray());
            float bestFitness = population[0].fitness;
            float worstFitness = population[populationSize-1].fitness;
            foreach (var individual in population)
            {
                ColoringComponent comp = individual.system.rootGameObject.GetComponentInChildren<ColoringComponent>();
                if (comp != null)
                {
                    comp.ApplyColor((worstFitness - individual.fitness) / (worstFitness - bestFitness));
                }
            }

        }

    }

 
    /// <summary>
    /// Cost function for one set of parameters, how far it is from final solution
    /// </summary>
    /// <returns></returns>
    public float CostFunction(List<float> parameters,List<float> goal,CostFunctionType type)
    {
        switch (type)
        {
            case CostFunctionType.MSE:
                float sumOfSquares = 0f;
                for (int i = 0; i < parameters.Count; i++)
                {
                    float difference = parameters[i] - goal[i];
                    sumOfSquares += difference * difference;
                }
                return sumOfSquares/parameters.Count;
                break;
            case CostFunctionType.Simple:
                float diff = 0f;
                for (int i = 0; i < parameters.Count; i++)
                {
                    float difference = parameters[i] - goal[i];
                    diff += difference;
                }
                return Mathf.Sqrt(diff);
            default:
                Debug.LogError("No cost type given!!! Using simple");
                return 0;
        }
    }

    /// <summary>
    /// Gets the best individual in the population.
    /// </summary>
    public Individual GetBestIndividual()
    {
        return population.OrderBy(ind => ind.fitness).First();
    }

    public enum CostFunctionType
    {
        Simple,
        MSE,
    } 
}
