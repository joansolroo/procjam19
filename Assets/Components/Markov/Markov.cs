using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Markov : MonoBehaviour
{
    public class MarkovNavigation
    {
        public int current;
        public Graph graph;

        public MarkovNavigation(Graph graph)
        {
            ChangeGraph(graph);
        }
        public void ChangeGraph(Graph graph)
        {
            this.graph = graph;
            this.current = graph.entry;
        }
        public int Navigate()
        {
            var currentTransitions = graph.transitions[current];
           
            //Computes the total cummulative probability (in case we are lazy and do not normalize) 
            float total = 0;
            foreach (float p in currentTransitions.probabilities)
            {
                total += p;
            }
            // if the chance of transition is larger than zero, moves forward, even to itself
            // otherwise, just keeps the last state
            if (total > 0)
            {
                // Montecarlo sampling
                float picked = Random.Range(0, total);
                float cumulative = 0;
                int next = 0;
                foreach (float p in currentTransitions.probabilities)
                {
                    cumulative += p;
                    if (cumulative > picked)
                    {
                        break;
                    }
                    else
                    {
                        ++next;
                    }
                }
                // save and apply result
                current = next;
            }
            return current;
        }
    }
    [SerializeField] Clock metronome;
    [SerializeField] Song song;
    [SerializeField] AudioSource source;
    [SerializeField] AudioClip[] clip;

    // Start is called before the first frame update
    void Start()
    {
        metronome.OnMayorTick += TickMayor;
        metronome.OnTick += Tick;
        navigationPart = new MarkovNavigation(song.transitions);
       
        ResetChannelNavigation();
        /*
        // debug mode
        // Creates a crappy song to test transitions
        song = new Song();
        int n = clip.Length;
        //song.channels = new SongChannel[n];
        song.transitions = new Graph(n);
        song.transitions.origin = new Graph.Links[n];
        for (int s = 0; s < n; ++s)
        {
            song.transitions.origin[s] = new Graph.Links();
            song.transitions.origin[s].probabilities = new float[n];
            for (int s2 = 0; s2 < n; ++s2)
            {
                song.transitions.origin[s].probabilities[s2] = Random.value;
            }
        }
        currentRowIdx = song.transitions.entry;
        */
    }
    MarkovNavigation navigationPart;
    MarkovNavigation[] navigationChannel;
    void ResetChannelNavigation()
    {
        navigationChannel = new MarkovNavigation[song.parts[navigationPart.current].channels.Length];
        for (int c = 0; c < song.parts[navigationPart.current].channels.Length; ++c)
        {
            navigationChannel[c] = new MarkovNavigation(song.parts[navigationPart.current].channels[c].transitions);
        }
    }
    void TickMayor()
    {
        int current = navigationPart.current;
        int next = navigationPart.Navigate();
        Debug.Log("PART Transition:" + current + "->" + next);

        if (current != next)
        {
            ResetChannelNavigation();
        }
    }
    void Tick()
    {
        SongPart part = song.parts[navigationPart.current];
        for (int c = 0; c < part.channels.Length; ++c)
        {
            int current = navigationPart.current;
            int next = navigationChannel[c].Navigate();
            Debug.Log("> CHANNEL:"+c+", Transition:" + current + "->" + next);           
            source.PlayOneShot(part.channels[c].clips[next]);
        }
    }
}
