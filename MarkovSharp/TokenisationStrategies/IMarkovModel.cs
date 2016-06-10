using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovSharp.TokenisationStrategies
{
    public interface IMarkovModel<TPhrase, TGram>
    {
        IEnumerable<TGram> SplitTokens(TPhrase input);
        
        TPhrase RebuildPhrase(IEnumerable<TGram> tokens);

        void Learn(IEnumerable<TPhrase> phrases, bool ignoreAlreadyLearnt = true);
        
        void Learn(TPhrase phrase);
        
        void Retrain(int newLevel);
        
        IEnumerable<TPhrase> Walk(int lines = 1, TPhrase seed = default(TPhrase));
        
        List<TGram> GetMatches(TPhrase input);
        
        void Save(string file);
        
        IMarkovModel<TPhrase, TGram> Load(string file, int level = 1);

        //int GetLevel();
    }
}
