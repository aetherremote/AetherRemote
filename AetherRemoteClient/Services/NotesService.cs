using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using AetherRemoteClient.Infrastructure.Database;

namespace AetherRemoteClient.Services;

/// <summary>
///     Provides access to any notes the user has 
/// </summary>
public class NotesService(DatabaseInfrastructure database)
{
    private Dictionary<string, string> _notes = [];
    private bool _dirty;

    /// <summary> List of all notes the client has </summary>
    public ImmutableDictionary<string, string> Notes
    {
        get
        {
            if (_dirty is false)
                return field;

            field = _notes.ToImmutableDictionary();
            _dirty = false;

            return field;
        }
    } = [];

    /// <summary> Load all the notes from the database </summary>
    /// <remarks> Call this once when the plugin loads initially </remarks>
    public async Task<bool> LoadNotes()
    {
        if (await database.GetNotes().ConfigureAwait(false) is not { } notes)
            return false;

        _notes = notes;
        _dirty = true;
        return true;
    }

    /// <summary>
    ///     Add a note
    /// </summary>
    public async Task<bool> AddNote(string friendCode, string note)
    {
        if (await database.SetNote(friendCode, note).ConfigureAwait(false) is false)
            return false;
        
        _notes[friendCode] = note;
        _dirty = true;
        return true;
    }

    /// <summary>
    ///     Remove a note
    /// </summary>
    public async Task<bool> RemoveNote(string friendCode)
    {
        if (await database.RemoveNote(friendCode).ConfigureAwait(false) is false)
            return false;

        _notes.Remove(friendCode);
        _dirty = true;
        return true;
    }
}