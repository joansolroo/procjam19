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
        metronome.OnTick += Tick;
        navigationPart = new MarkovNavigation(song.transitions);
       
        ResetPartProgress();

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
    public int partBeat;
    MarkovNavigation[] navigationChannel;
    public int[] channelBeat;
    SongPart currentPart;
    void ResetPartProgress()
    {
        partBeat = 0;
        currentPart = song.parts[navigationPart.current];

        navigationChannel = new MarkovNavigation[currentPart.channels.Length];
        channelBeat = new int[currentPart.channels.Length];
        for (int c = 0; c < currentPart.channels.Length; ++c)
        {
            navigationChannel[c] = new MarkovNavigation(currentPart.channels[c].transitions);
            channelBeat[c] = 0;
        }
    }

    bool first = true;
    void Tick()
    {
        bool resetted = false;
        if(first)
        {
            resetted = true;
            first = false;
        }
        if(partBeat >= currentPart.duration)
        {
            int current = navigationPart.current;
            int next = navigationPart.Navigate();
            Debug.Log("PART Transition:" + current + "->" + next);

            if (current != next)
            {
                ResetPartProgress();
                resetted = true;
            }
            partBeat = 0;
        }
        ++partBeat;
        
        for (int c = 0; c < currentPart.channels.Length; ++c)
        {
            SongChannel channel = currentPart.channels[c];
            int current = navigationChannel[c].current;
            if (resetted)
            {
                if (channel.clips[current] != null)
                {
                    source.PlayOneShot(channel.clips[current]);
                }
                Debug.Log("> CHANNEL:" + c + ", enter:" + current);
            }
            if(channelBeat[c]>= channel.duration)
            {
                int next = navigationChannel[c].Navigate();
                Debug.Log("> CHANNEL:" + c + ", Transition:" + current + "->" + next);
                if (channel.clips[next] != null)
                {
                    source.PlayOneShot(channel.clips[next]);
                }

                channelBeat[c] = 0;
            }
            ++channelBeat[c];
        }
    }
}
