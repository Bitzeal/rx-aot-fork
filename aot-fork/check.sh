#!/bin/sh
# Fails when the fork change is missing, e.g. a new upstream operator still has `public virtual`.
set -eu
here="$(cd "$(dirname "$0")" && pwd)"
linq="$here/../Rx.NET/Source/src/System.Reactive/Linq"
cd "$linq"
fail=0
grep -n 'public virtual ' QueryLanguage*.cs QueryLanguage*.tt && { echo "check: public virtual left in QueryLanguage"; fail=1; }
grep -q 'private static readonly QueryLanguage s_impl = new();' Observable_.cs || { echo "check: Observable_.cs still goes through IQueryLanguage"; fail=1; }
grep -q 'private static readonly QueryLanguageEx s_impl = new();' ObservableEx.cs || { echo "check: ObservableEx.cs still goes through IQueryLanguageEx"; fail=1; }
grep -q 'internal sealed partial class QueryLanguage$' QueryLanguage_.cs || { echo "check: QueryLanguage is not sealed"; fail=1; }
grep -q 'internal sealed partial class QueryLanguageEx$' QueryLanguageEx.cs || { echo "check: QueryLanguageEx is not sealed"; fail=1; }
[ "$fail" = 0 ] && echo "check: fork change complete"
exit "$fail"
