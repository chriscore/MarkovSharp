using System.Collections.Generic;

namespace MarkovSharp.TokenisationStrategies
{
    public interface IMarkovStrategy<TPhrase, TGram>
    {
        IEnumerable<TGram> SplitTokens(TPhrase input);
        
        TPhrase RebuildPhrase(IEnumerable<TGram> tokens);

        void Learn(IEnumerable<TPhrase> phrases, bool ignoreAlreadyLearnt = true);
        
        void Learn(TPhrase phrase);
        
        void Retrain(int newLevel);
        
        IEnumerable<TPhrase> Walk(int lines = 1, TPhrase seed = default(TPhrase));
        
        List<TGram> GetMatches(TPhrase input);
        
        void Save(string file);

        T Load<T>(string file, int level = 1) where T : IMarkovStrategy<TPhrase, TGram>;

        TGram GetTerminatorGram();

        TGram GetPrepadGram();
        //int GetLevel();
    }
}
