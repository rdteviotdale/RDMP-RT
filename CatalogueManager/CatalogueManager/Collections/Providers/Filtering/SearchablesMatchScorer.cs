﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CatalogueLibrary.Providers;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTable;
using Microsoft.SqlServer.Management.Smo;
using ReusableLibraryCode;

namespace CatalogueManager.Collections.Providers.Filtering
{
    public class SearchablesMatchScorer
    {
        private static readonly int[] Weights = new int[] { 64, 32, 16, 8, 4, 2, 1 };
        
        private List<RDMPCollection> _showOnlyCollections;
        private IActivateItems _activator;


        public string[] TypeNames { get; set; }

        public SearchablesMatchScorer()
        {
            TypeNames = new string[0];
        }

        public Dictionary<KeyValuePair<IMapsDirectlyToDatabaseTable, DescendancyList>, int> ScoreMatches(Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> searchables, string searchText, CancellationToken cancellationToken)
        {
            var tokens = (searchText??"").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            var regexes = new List<Regex>();
            
            //any token that 100% matches a type name is an explicitly typed token
            IEnumerable<string> explicitTypesRequested;

            if (TypeNames != null)
            {
                explicitTypesRequested = TypeNames.Intersect(tokens);

                //else it's a regex
                foreach (string token in tokens.Except(TypeNames))
                    regexes.Add(new Regex(Regex.Escape(token), RegexOptions.IgnoreCase));

            }
            else
                explicitTypesRequested = new string[0];

            if (cancellationToken.IsCancellationRequested)
                return null;

            return searchables.ToDictionary(
           s => s,
           score => ScoreMatches(score, regexes,explicitTypesRequested.ToArray(), cancellationToken)
           );
        }

        private int ScoreMatches(KeyValuePair<IMapsDirectlyToDatabaseTable, DescendancyList> kvp, List<Regex> regexes, string[] explicitTypeNames, CancellationToken cancellationToken)
        {
            int score = 0;

            if (cancellationToken.IsCancellationRequested)
                return 0;

            if (explicitTypeNames.Any())
                if (!explicitTypeNames.Contains(kvp.Key.GetType().Name))
                    return 0;

            //if we are only to show results for objects which are in the supplied RDMPCollections 
            if (_showOnlyCollections != null)
            {
                //root is either the first parent or the object itself (if no descendancy)
                var root = kvp.Value != null && kvp.Value.Parents != null && kvp.Value.Parents.Any()
                    ? kvp.Value.Parents[0]
                    : kvp.Key;

                //if none of the filtered collections own the root object
                if(!_showOnlyCollections.Any(c=>_activator.IsRootObjectOfCollection(c,root)))
                      return 0;
            }


            //don't suggest AND/OR containers it's not helpful to navigate to these
            if (kvp.Key is CatalogueLibrary.Data.IContainer)
                return 0;

            //if there are no tokens
            if (!regexes.Any())
                if (explicitTypeNames.Any()) //if they have so far just typed a TypeName
                    return 1;
                else
                    return 1;//no regexes AND no TypeName what did they type! whatever everyone scores the same
            
            //make a new list so we can destructively read it
            regexes = new List<Regex>(regexes);
            
            //match on the head vs the regex tokens
            score += Weights[0] * CountMatchToString(regexes, kvp.Key);

            score += Weights[0] * CountMatchType(regexes, kvp.Key);

            //match on the parents if theres a decendancy list
            if (kvp.Value != null)
            {
                var parents = kvp.Value.Parents;
                int numberOfParents = parents.Length;

                //for each prime after the first apply it as a multiple of the parent match
                for (int i = 1; i < Weights.Length; i++)
                {
                    //if we have run out of parents
                    if (i > numberOfParents)
                        break;

                    var parent = parents[parents.Length - i];

                    if (parent != null)
                    {
                        if (!(parent is CatalogueLibrary.Data.IContainer))
                        {
                            score += Weights[i] * CountMatchToString(regexes, parent);
                            score += Weights[i] * CountMatchType(regexes, parent);
                        }
                    }
                }
            }

            //if there were unmatched regexes
            if (regexes.Any())
                return 0;

            return score;
        }

        private int CountMatchType(List<Regex> regexes, object key)
        {
            return MatchCount(regexes, key.GetType().Name);
        }
        private int CountMatchToString(List<Regex> regexes, object key)
        {
            var s = key as ICustomSearchString;
            string matchOn = s != null ? s.GetSearchString() : key.ToString();

            return MatchCount(regexes, matchOn);
        }
        private int MatchCount(List<Regex> regexes, string str)
        {
            int matches = 0;
            foreach (var match in regexes.Where(r => r.IsMatch(str)).ToArray())
            {
                regexes.Remove(match);
                matches++;
            }

            return matches;
        }

        public void OnlyShowMatches(List<RDMPCollection> showOnlyCollections, IActivateItems activator)
        {
            _showOnlyCollections = showOnlyCollections;
            _activator = activator;
        }
    }
}
