﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData;
using OData.Linq.Expressions;
using OData.Linq.Fluent;

namespace OData.Linq
{
    public class CommandFormatter : CommandFormatterBase
    {
        private const string StarString = "*";

        public CommandFormatter(ISession session)
            : base(session)
        {
        }

        public override FunctionFormat FunctionFormat => FunctionFormat.Key;

        public override string ConvertValueToUriLiteral(object value, bool escapeDataString)
        {
            var type = value?.GetType();

            if (value != null && _session.TypeCache.IsEnumType(type))
                value = new ODataEnumValue(value.ToString(), type.FullName); // _session.Metadata.GetQualifiedTypeName(type.Name));
            if (value is ODataExpression expression)
                return expression.AsString(_session);

            var odataVersion = (ODataVersion)Enum.Parse(typeof(ODataVersion), _session.Adapter.GetODataVersionString(), false);
            string ConvertValue(object x) => ODataUriUtils.ConvertToUriLiteral(x, odataVersion, null);

            if (value is ODataEnumValue && _session.Settings.EnumPrefixFree)
                value = ((ODataEnumValue) value).Value;
            else if (value is DateTime)
                value = new DateTimeOffset((DateTime)value);

            return escapeDataString
                ? Uri.EscapeDataString(ConvertValue(value))
                : ConvertValue(value);
        }

        protected override void FormatExpandSelectOrderby(IList<string> commandClauses, EntityCollection resultCollection, ResolvedCommand command)
        {
            //if (command.Details.ExpandAssociations.Any())
            //{
            //    var groupedExpandAssociations = command.Details.ExpandAssociations
            //        .GroupBy(x => (x.Key, x.Value), x => x.Key);
            //    var mergedExpandAssociations = groupedExpandAssociations
            //        .Select(x =>
            //        {
            //            var mainAssociation = x.Key.Key;
            //            foreach (var association in x.Where(a => a != mainAssociation))
            //            {
            //                mainAssociation = MergeExpandAssociations(mainAssociation, association).First();
            //            }
            //            return new KeyValuePair<ODataExpandAssociation, ODataExpandOptions>(mainAssociation, x.Key.Value);
            //        });
                
            //    var formattedExpand = string.Join(",", mergedExpandAssociations.Select(x =>
            //        FormatExpansionSegment(x.Key, resultCollection, x.Value, command)));
            //    commandClauses.Add($"{ODataLiteral.Expand}={formattedExpand}");
            //}

            //FormatClause(commandClauses, resultCollection,
            //    SelectPathSegmentColumns(command.Details.SelectColumns, null, resultCollection,
            //        command.Details.ExpandAssociations.Select(x => FormatFirstSegment(x.Key.Name)).ToList()),
            //    ODataLiteral.Select, FormatSelectItem);

            //FormatClause(commandClauses, resultCollection,
            //    command.Details.OrderbyColumns
            //        .Where(o => !command.Details.ExpandAssociations.Select(ea => ea.Key)
            //                    .Any(ea => IsInnerCollectionOrderBy(ea.Name, resultCollection, o.Key))).ToList(),
            //    ODataLiteral.OrderBy, FormatOrderByItem);
        }

        protected override void FormatInlineCount(IList<string> commandClauses)
        {
            commandClauses.Add($"{ODataLiteral.Count}={ODataLiteral.True}");
        }

        //private string FormatExpansionSegment(ODataExpandAssociation association, EntityCollection entityCollection,
        //    ODataExpandOptions expandOptions, ResolvedCommand command, bool rootLevel = true)
        //{
        //    if (rootLevel)
        //    {
        //        association = command.Details.SelectColumns.Aggregate(association, MergeExpandAssociations);
        //        association = command.Details.OrderbyColumns.Aggregate(association, MergeOrderByColumns);
        //    }

        //    var associationName = association.Name;
        //    var expandsToCollection = false;
        //    if (_session.Metadata.HasNavigationProperty(entityCollection.Name, associationName))
        //    {
        //        associationName = _session.Metadata.GetNavigationPropertyExactName(entityCollection.Name, associationName);
        //        expandsToCollection = _session.Metadata.IsNavigationPropertyCollection(entityCollection.Name, associationName);
        //    }

        //    var clauses = new List<string>();
        //    var text = associationName;
        //    if (expandOptions.ExpandMode == ODataExpandMode.ByReference)
        //        text += "/" + ODataLiteral.Ref;

        //    if (expandOptions.Levels > 1)
        //    {
        //        clauses.Add($"{ODataLiteral.Levels}={expandOptions.Levels}");
        //    }
        //    else if (expandOptions.Levels == 0)
        //    {
        //        clauses.Add($"{ODataLiteral.Levels}={ODataLiteral.Max}");
        //    }

        //    if (associationName != StarString)
        //    {
        //        if (expandsToCollection && !ReferenceEquals(association.FilterExpression, null))
        //        {
        //            var associatedEntityCollection = _session.Metadata.GetEntityCollection(
        //                _session.Metadata.GetNavigationPropertyPartnerTypeName(entityCollection.Name, associationName));
        //            clauses.Add(
        //                $"{ODataLiteral.Filter}={EscapeUnescapedString(association.FilterExpression.Format(new ExpressionContext(_session, associatedEntityCollection, null, command.DynamicPropertiesContainerName)))}");
        //        }

        //        if (association.ExpandAssociations.Any())
        //        {
        //            var associatedEntityCollection = _session.Metadata.GetEntityCollection(
        //                _session.Metadata.GetNavigationPropertyPartnerTypeName(entityCollection.Name, associationName));
        //            var expandAll = association.ExpandAssociations.FirstOrDefault(a => a.Name == StarString);
        //            if (expandAll != null)
        //            {
        //                clauses.Add($"{ODataLiteral.Expand}=*");
        //            }
        //            else
        //            {
        //                var expandedProperties = string.Join(",", association.ExpandAssociations
        //                    .Where(
        //                        a => _session.Metadata.HasNavigationProperty(associatedEntityCollection.Name, a.Name))
        //                    .Select(a =>
        //                        FormatExpansionSegment(a, associatedEntityCollection, ODataExpandOptions.ByValue(),
        //                            command,
        //                            false)));
        //                if (!string.IsNullOrEmpty(expandedProperties))
        //                    clauses.Add($"{ODataLiteral.Expand}={expandedProperties}");
        //            }

        //            var selectColumns = string.Join(",", association.ExpandAssociations
        //                .Where(a => a.Name != StarString &&
        //                            !_session.Metadata.HasNavigationProperty(associatedEntityCollection.Name, a.Name))
        //                .Select(a => a.Name));
        //            if (!string.IsNullOrEmpty(selectColumns))
        //                clauses.Add($"{ODataLiteral.Select}={selectColumns}");
        //        }

        //        if (expandsToCollection && association.OrderByColumns.Any())
        //        {
        //            var columns = string.Join(",", association.OrderByColumns
        //                .Select(o => o.Name + (o.Descending ? " desc" : string.Empty)));
        //            if (!string.IsNullOrEmpty(columns))
        //                clauses.Add($"{ODataLiteral.OrderBy}={columns}");
        //        }
        //    }

        //    if (clauses.Any())
        //        text += $"({string.Join(";", clauses)})";

        //    return text;
        //}

        private static ODataExpandAssociation MergeExpandAssociations(ODataExpandAssociation expandAssociation, string path)
        {
            return MergeExpandAssociations(expandAssociation, ODataExpandAssociation.From(path)).First();
        }
        
        private static IEnumerable<ODataExpandAssociation> MergeExpandAssociations(ODataExpandAssociation first, ODataExpandAssociation second)
        {
            if (first.Name != second.Name && first.Name != "*") return new [] {first, second};

            var result = first.Clone();
            result.OrderByColumns.AddRange(second.OrderByColumns.Except(first.OrderByColumns));
            result.ExpandAssociations.Clear();
            var groupedExpandAssociations = first.ExpandAssociations
                .Concat(second.ExpandAssociations)
                .GroupBy(x => x);
            var mergedExpandAssociations = groupedExpandAssociations
                .Select(x =>
                {
                    var mainAssociation = x.Key;
                    foreach (var association in x.Where(a => a != mainAssociation))
                    {
                        mainAssociation = MergeExpandAssociations(mainAssociation, association).First();
                        mainAssociation.OrderByColumns.AddRange(association.OrderByColumns.Except(mainAssociation.OrderByColumns));
                    }
                    return mainAssociation;
                });
            
            result.ExpandAssociations.AddRange(mergedExpandAssociations);
            
            return new [] {result};
        }

        private static ODataExpandAssociation MergeOrderByColumns(ODataExpandAssociation expandAssociation, KeyValuePair<string, bool> orderByColumn)
        {
            if (string.IsNullOrEmpty(orderByColumn.Key))
                return expandAssociation;
            
            var segments = orderByColumn.Key.Split('/');
            if (segments[0] != expandAssociation.Name)
                return expandAssociation;

            var result = expandAssociation.Clone();
            MergeOrderByColumns(result, segments, orderByColumn.Value, 1);
            return result;
        }

        private static void MergeOrderByColumns(ODataExpandAssociation expandAssociation,
            string[] segments, bool descending, int currentIndex)
        {
            if (segments.Length == currentIndex)
                return;

            if (segments.Length == currentIndex + 1)
            {
                expandAssociation.OrderByColumns.Add(new ODataOrderByColumn(segments[currentIndex], descending));
                return;
            }
            
            var nestedAssociation = expandAssociation.ExpandAssociations.FirstOrDefault(a => a.Name == segments[currentIndex]);
            if (nestedAssociation != null)
            {
                MergeOrderByColumns(nestedAssociation, segments, descending, currentIndex + 1);
            }
        }

        private IList<string> SelectPathSegmentColumns(
            IList<string> columns, string path, EntityCollection collection, IList<string> excludePaths = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                if (excludePaths != null)
                {
                    if (excludePaths.Count == 1 && excludePaths[0] == StarString)
                    {
                        var firstSegmentPropertyNames = new HashSet<string>(_session.Metadata.GetNavigationPropertyNames(collection.Name).Select(FormatFirstSegment));
                        return columns.Where(x => !firstSegmentPropertyNames.Any(y => y.Contains(FormatFirstSegment(x)))).ToList();
                    }

                    return columns.Where(x => !excludePaths.Any(y => FormatFirstSegment(y).Contains(FormatFirstSegment(x)))).ToList();
                }
            }

            var firstSegment = FormatFirstSegment(path);
            if (firstSegment == StarString)
            {
                var propertyNames = new HashSet<string>(_session.Metadata.GetNavigationPropertyNames(collection.Name));
                return columns
                    .Where(x => HasMultipleSegments(x) && propertyNames.Contains(FormatFirstSegment(x)))
                    .Select(x => FormatSkipSegments(x, 1)).ToList();
            }

            return columns
                .Where(x => HasMultipleSegments(x) && FormatFirstSegment(x) == firstSegment)
                .Select(x => FormatSkipSegments(x, 1)).ToList();
        }

        private bool IsInnerCollectionOrderBy(string expandAssociation, EntityCollection entityCollection, string orderByColumn)
        {
            var items = expandAssociation.Split('/');
            if (items.First() != FormatFirstSegment(orderByColumn))
                return false;

            var associationName = _session.Metadata.GetNavigationPropertyExactName(entityCollection.Name, items.First());
            if (_session.Metadata.IsNavigationPropertyCollection(entityCollection.Name, associationName))
                return true;

            if (items.Count() > 1)
            {
                expandAssociation = expandAssociation.Substring(items.First().Length + 1);
                entityCollection = _session.Metadata.GetEntityCollection(
                  _session.Metadata.GetNavigationPropertyPartnerTypeName(entityCollection.Name, associationName));

                if (!HasMultipleSegments(orderByColumn) || FormatFirstSegment(orderByColumn) != FormatFirstSegment(expandAssociation))
                    return false;

                orderByColumn = FormatSkipSegments(orderByColumn, 1);
                return IsInnerCollectionOrderBy(expandAssociation, entityCollection, orderByColumn);
            }

            return false;
        }

        private bool HasMultipleSegments(string path)
        {
            return path.Contains("/");
        }

        private string FormatFirstSegment(string path)
        {
            return path.Split('/').First();
        }

        private string FormatSkipSegments(string path, int skipCount)
        {
            return string.Join("/", path.Split('/').Skip(skipCount));
        }
    }
}