using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Markov : MonoBehaviour
{
    public class MarkovNavigation
    {
        public int current;
        public GraphDense graph;
       
        public MarkovNavigation(GraphDense graph)
        {
            ChangeGraph(graph);
        }
        public void ChangeGraph(GraphDense graph)
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
   
    [SerializeField] Dictionary<UnityEngine.Audio.AudioMixerGroup, AudioSource> sources = new Dictionary<UnityEngine.Audio.AudioMixerGroup, AudioSource>();
    AudioSource defaultSource;

    [Header("Status")]
    [SerializeField] SongPart currentPart;
    MarkovNavigation navigationPart;
    MarkovNavigation[] navigationChannel;
    [SerializeField] int partBeat;
    [SerializeField] int[] channelBeat;
   
    // Start is called before the first frame update
    void Start()
    {
        metronome.OnTick += Tick;
        navigationPart = new MarkovNavigation(song.transitions);
        defaultSource = GetComponent<AudioSource>();
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
        if (currentPart.mixerSnapshot != null)
        {
            currentPart.mixerSnapshot.TransitionTo(0);
        }
    }

    bool first = true;
    bool initialized = false;
    void Tick()
    {
        if (!initialized)
        {
            initialized = true;
            return;
        }
        bool resetted = false;
        if(first)
        {
            resetted = true;
            first = false;
            ResetPartProgress();
        }
        else if(partBeat >= currentPart.duration)
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
            AudioClip toPlay = null;
            if (resetted)
            {
                if (channel.clips[current] != null)
                {
                    toPlay = channel.clips[current];
                }
                Debug.Log("> CHANNEL:" + c + ", enter:" + current);
            }
            // TODO: improve. This double if is because the first time a part loops it has a missing beat
            if((channelBeat[c]>= channel.duration))
            {
                int next = navigationChannel[c].Navigate();
                Debug.Log("> CHANNEL:" + c + ", Transition:" + current + "->" + next);
                if (channel.clips[next] != null)
                {
                    toPlay = channel.clips[next];
                }

                channelBeat[c] = 0;
            }
            if (toPlay != null)
            {
                AudioSource source;
                var mixerGroup = channel.mixerGroup;
                if(mixerGroup== null)
                {
                    source = defaultSource;
                }
                else
                {
                    if (sources.ContainsKey(mixerGroup))
                    {
                        source = sources[mixerGroup];
                    }
                    else
                    {
                        source = gameObject.AddComponent<AudioSource>();
                        source.outputAudioMixerGroup = mixerGroup;
                        sources[mixerGroup] = source;
                    }
                }
                source.pitch = metronome.BPM / song.BPM;
                source.PlayOneShot(toPlay);
            }
            ++channelBeat[c];
        }
    }
}
