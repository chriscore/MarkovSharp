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

        /// <summary>Loads a strategy from a file with the given filename.</summary>
        /// <typeparam name="T">The type of <see cref="IMarkovStrategy{TPhrase,TGram}"/></typeparam>
        /// <param name="file">The name of the file to load.</param>
        /// <param name="level">The level.</param>
        /// <returns></returns>
        T Load<T>(string file, int level = 1) where T : IMarkovStrategy<TPhrase, TGram>;

        TGram GetTerminatorGram();

        TGram GetPrepadGram();
    }
}
