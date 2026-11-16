#!/bin/sh
# Turns upstream System.Reactive into the AOT fork. Run once on a fresh upstream tag.
# Observable calls the sealed QueryLanguage directly instead of through IQueryLanguage,
# so NativeAOT does not have to build generic virtual method code at first use.
# The public API does not change.
set -eu
here="$(cd "$(dirname "$0")" && pwd)"
linq="$here/../Rx.NET/Source/src/System.Reactive/Linq"
cd "$linq"

perl -pi -e 's|private static IQueryLanguage s_impl = QueryServices\.GetQueryImpl<IQueryLanguage>\(new QueryLanguage\(\)\);|private static readonly QueryLanguage s_impl = new();|' Observable_.cs
perl -pi -e 's|private static IQueryLanguageEx s_impl = QueryServices\.GetQueryImpl<IQueryLanguageEx>\(new QueryLanguageEx\(\)\);|private static readonly QueryLanguageEx s_impl = new();|' ObservableEx.cs
perl -pi -e 's|internal partial class QueryLanguage : IQueryLanguage\b|internal sealed partial class QueryLanguage|' QueryLanguage_.cs
perl -pi -e 's|internal partial class QueryLanguageEx : IQueryLanguageEx\b|internal sealed partial class QueryLanguageEx|' QueryLanguageEx.cs
perl -pi -e 's|public virtual |public |g' QueryLanguage*.cs QueryLanguage*.tt

"$here/check.sh"
