using System.Collections.Generic;

namespace MarkovSharp.TokenisationStrategies
{
    /// <summary>
    /// An implementation of the Markov Model Engine.
    /// </summary>
    /// <typeparam name="TPhrase">The type of the phrase.</typeparam>
    /// <typeparam name="TGram">The type of the gram.</typeparam>
    public interface IMarkovStrategy<TPhrase, TGram>
    {
        /// <summary>Defines how to split the phrase to ngrams</summary>
        /// <param name="input">The phrase to split</param>
        IEnumerable<TGram> SplitTokens(TPhrase input);

        /// <summary>Defines how to join ngrams back together to form a phrase</summary>
        TPhrase RebuildPhrase(IEnumerable<TGram> tokens);

        void Learn(IEnumerable<TPhrase> phrases, bool ignoreAlreadyLearnt = true);
        
        void Learn(TPhrase phrase);

        /// <summary>Retrain an existing trained model instance to a different 'level'</summary>
        void Retrain(int newLevel);

        /// <summary>Generate a collection of phrase output data based on the current model</summary>
        /// <param name="lines">The number of phrases to emit</param>
        /// <param name="seed">Optionally provide the start of the phrase to generate from</param>
        IEnumerable<TPhrase> Walk(int lines = 1, TPhrase seed = default(TPhrase));

        /// <summary>Returns any viable options for the next word based on
        /// what was provided as input, based on the trained model.</summary>
        /// <param name="input">The input.</param>
        List<TGram> GetMatches(TPhrase input);

        /// <summary>Seriaize the model to file for use later</summary>
        string Serialize();

        /// <summary>Load a model which has been saved</summary>
        /// <typeparam name="T">The type of markov model to load the data as</typeparam>
        /// <param name="file">The serialized model data</param>
        /// <param name="level">The level to apply to the loaded model (model will be trained on load)</param>
        T Deserialize<T>(string file, int level = 1) where T : IMarkovStrategy<TPhrase, TGram>;

        TGram GetTerminatorGram();

        TGram GetPrepadGram();
    }
}
