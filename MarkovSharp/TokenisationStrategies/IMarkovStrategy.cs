using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkovSharp.Models;

namespace MarkovSharp.TokenisationStrategies
{
    public interface IMarkovStrategy<TPhrase, TUnigram>
    {
        Type UnigramType { get; }

        Type PhraseType { get; }

        IEnumerable<TUnigram> SplitTokens(TPhrase input);
        
        TPhrase RebuildPhrase(IEnumerable<TUnigram> tokens);

        void Learn(IEnumerable<TPhrase> phrases, bool ignoreAlreadyLearnt = true);
        
        void Learn(TPhrase phrase);
        
        void Retrain(int newLevel);
        
        IEnumerable<TPhrase> Walk(int lines = 1, TPhrase seed = default(TPhrase));
        
        List<TUnigram> GetMatches(TPhrase input);

        TUnigram GetTerminatorUnigram();

        TUnigram GetPrepadUnigram();

        IEnumerable<StateStatistic<TUnigram>> GetStatistics();

        ChainPhraseProbability<TPhrase> GetFit(TPhrase test);

        double GetTransitionProbabilityUnigram(TPhrase currentState, TUnigram nextStates);

        double GetTransitionProbabilityPhrase(TPhrase currentState, TPhrase nextStates);
    }
}
