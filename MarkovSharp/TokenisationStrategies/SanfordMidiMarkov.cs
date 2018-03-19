using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovSharp.TokenisationStrategies
{
    public class SanfordMidiMarkov : GenericMarkov<Track, Note>
    {
        public SanfordMidiMarkov(int level = 2)
            : base(level)
        {
            this.Channel = 1;
        }

        public int Channel { get; set; }

        public override Note GetTerminatorUnigram()
        {
            return null;
        }

        public override Note GetPrepadUnigram()
        {
            return new Note();
        }

        public override IEnumerable<Note> SplitTokens(Track input)
        {
            return ParseTrack(input);
        }

        public override Track RebuildPhrase(IEnumerable<Note> tokens)
        {
            // timing reconstructing the phrase makes a big difference currently
            // we need to set the position (start) of a note dynamically, as otherwise
            // notes will be placed exactly in the arrangement where they were before,
            // just with some missing.
            var t = new Track();
            int pos = 0;
            
            foreach (var token in tokens)
            {
                if (token != null)
                {
                    pos = pos + token.TimeSinceLastEvent;
                    t.Insert(Math.Abs(pos), new ChannelMessage(token.Command, Channel, token.Pitch, token.Velocity));
                    t.Insert(Math.Abs(pos + token.Duration), new ChannelMessage(token.Command, Channel, token.Pitch, 0));
                    pos++;
                }
            }

            return t;
        }

        public List<Note> ParseTrack(Track track)
        {
            if (track == null)
            {
                return new List<Note>();
            }

            List<NoteEvent> notes = new List<NoteEvent>();
            
            for (int i = 0; i < track.Count; i++)
            {
                var midiEvent = track.GetMidiEvent(i);
                if (midiEvent.MidiMessage.MessageType == MessageType.Channel)
                {
                    var m = (ChannelMessage)midiEvent.MidiMessage;
                    if (m.Command == ChannelCommand.NoteOn || m.Command == ChannelCommand.NoteOff || m.Command == ChannelCommand.ProgramChange  )
                    {
                        Channel = m.MidiChannel;
                        notes.Add(new NoteEvent { Pitch = m.Data1, Velocity = m.Data2, TimeStamp = midiEvent.AbsoluteTicks, Command = m.Command});
                    }
                }
            }

            return PairUpNoteEvents(notes);
        }


        private List<Note> PairUpNoteEvents(List<NoteEvent> notes)
        {
            List<Note> builtList = new List<Note>();

            var noteArray = notes.ToArray();

            int lastTimestamp = 0;

            for (int i = 0; i < noteArray.Length; i++)
            {
                var current = noteArray[i];
                //current.Dump($"current ({i})");
                if (current == null)
                {
                    continue;
                }


                if (current.Command == ChannelCommand.NoteOn)
                {
                    // collect and remove the corresponding note off message
                    for (int j = 0; j < noteArray.Length; j++)
                    {
                        if (noteArray[j]?.Pitch == current?.Pitch && noteArray[j]?.Velocity == 0)
                        {
                            var paired = noteArray[j];
                            //paired.Dump($"paired ({j})");

                            if (paired != null)
                            {
                                builtList.Add(new Note
                                {
                                    Pitch = current.Pitch,
                                    Velocity = current.Velocity,
                                    Duration = paired.TimeStamp - current.TimeStamp,
                                    StartTime = current.TimeStamp,
                                    Command = current.Command,
                                    TimeSinceLastEvent = current.TimeStamp - lastTimestamp
                                });
                                noteArray[j] = null;
                                noteArray[i] = null;
                            }
                            else
                            {
                                //current.Dump();
                                throw new Exception("Pair not found for note event");
                            }
                            break;
                        }
                    }
                }
                else
                {
                    builtList.Add(new Note
                    {
                        Command = current.Command,
                        Duration = 0,
                        Pitch = current.Pitch,
                        StartTime = current.TimeStamp,
                        Velocity = current.Velocity,
                        TimeSinceLastEvent = current.TimeStamp - lastTimestamp
                    });
                }

                lastTimestamp = current.TimeStamp;
            }

            return builtList;
        }
    }


    public class NoteEvent
    {
        public int Pitch { get; set; }
        public int Velocity { get; set; }
        public int TimeStamp { get; set; }
        public ChannelCommand Command { get; set; }

        public override bool Equals(object o)
        {
            var x = o as NoteEvent;
            if (x == null && this != null)
            {
                return false;
            }

            var equals =
            (
                this.Pitch == x.Pitch
                &&
                ((Velocity != 0 && x.Velocity != 0) || (Velocity == 0 && x.Velocity == 0))
                &&
                Command == x.Command
            );

            return equals;
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Pitch.GetHashCode();
                hash = hash * 7 + (Velocity == 0).GetHashCode();
                //hash = hash * 23 + Duration.GetHashCode();
                return hash;
            }
        }
    }

    public class Note : IComparable
    {
        public int Pitch { get; set; }
        public int Velocity { get; set; }
        public int Duration { get; set; }
        public int StartTime { get; set; }
        public int TimeSinceLastEvent { get; set; }
        public ChannelCommand Command { get; set; }

        public override bool Equals(object o)
        {
            var x = o as Note;
            if (x == null && this != null)
            {
                return false;
            }

            var equals =
            (
                this.Pitch == x.Pitch
                &&
                ((Velocity != 0 && x.Velocity != 0) || (Velocity == 0 && x.Velocity == 0))
            //&&
            //Duration == x.Duration
            );

            return equals;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Pitch.GetHashCode();
                hash = hash * 13 + (Velocity == 0).GetHashCode();
                //hash = hash * 23 + Duration.GetHashCode();
                return hash;
            }
        }

        public int CompareTo(object obj)
        {
            var x = obj as Note;
            if (x == null && this != null)
            {
                return 0;
            }

            return this.Pitch.CompareTo(x.Pitch);
        }
    }
}
