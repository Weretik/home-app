namespace Notes.Domain.Notes;

/// <summary>What a note says about its person (R-002): «Любить», «Не любить» or a plain «Факт».</summary>
public enum NoteKind
{
    Likes,
    Dislikes,
    Fact,
}
