using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkovSharp.TokenisationStrategies;
using NUnit.Framework;
using Sanford.Multimedia.Midi;

namespace MarkovSharp.Tests
{
    [TestFixture]
    public class MidiMarkovTests
    {
        [Test]
        public void Testcase()
        {
            var midiFile = "swars.mid";

            Sequence s = new Sequence(midiFile);
            Sequence sNew = new Sequence(s.Division);

            var trackNumbers = s.Count;
            for (int i = 1; i < trackNumbers; i++)
            {
                Track t = s[i];
                SanfordMidiMarkov model = new SanfordMidiMarkov(2);
                model.Learn(t);

                var result = model.Walk(1);

                var built = result.First();
                
                sNew.Add(built);
            }
            sNew.Save("swarsNew.mid");
        }

        [Test]
        public void NoteEquals()
        {
            var a = new Note { Pitch = 60, Velocity = 127, Duration = 100, StartTime = 1234 };
            var a2 = new Note { Pitch = 60, Velocity = 127, Duration = 100, StartTime = 1234 };

            var b = new Note { Pitch = 60, Velocity = 0, Duration = 100, StartTime = 1234 };

            var c = new Note { Pitch = 60, Velocity = 127, Duration = 120, StartTime = 1234 };
            var d = new Note { Pitch = 60, Velocity = 127, Duration = 120, StartTime = 4321 };

            var e = new Note { Pitch = 61, Velocity = 127, Duration = 120, StartTime = 1234 };

            Assert.That(a.Equals(a2));
            Assert.That(a.Equals(c));
            Assert.That(a.Equals(d));

            Assert.That(!a.Equals(b));
            Assert.That(!b.Equals(c));
            Assert.That(!b.Equals(d));

            Assert.That(!a.Equals(e));

        }
    }
}
