// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>", Scope = "member", Target = "~M:Domain.EventSourcing.EventSourcedGrain`2.DoAndSave(System.Func{System.Collections.Generic.List{`1}},System.Action,System.Action)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>", Scope = "member", Target = "~M:Domain.EventSourcing.DataAccess.StreamStoreEventsDataAccess.SaveEvent(System.String,System.Object,System.Object,System.Int32)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>", Scope = "member", Target = "~M:Domain.EventSourcing.DataAccess.CommittedEventsPublisherGrain.StreamMessageReceived(SqlStreamStore.IAllStreamSubscription,SqlStreamStore.Streams.StreamMessage,System.Threading.CancellationToken)~System.Threading.Tasks.Task")]
