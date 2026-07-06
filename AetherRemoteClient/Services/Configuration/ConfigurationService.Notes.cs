using System.Collections.Generic;
using System.Threading.Tasks;

namespace AetherRemoteClient.Services.Configuration;

public partial class ConfigurationService
{
    private Dictionary<string, string> _notes = [];
    
    /// <summary> Get the note for provided friend code, if one exists </summary>
    public string? GetNoteFor(string friendCode) => _notes.TryGetValue(friendCode, out var value) ? value : null;

    /// <summary> Updates the database with a new note, or replaces the original note </summary>
    public async Task<bool> AddNote(string friendCode, string note)
    {
        if (await databaseInfrastructure.SetNote(friendCode, note).ConfigureAwait(false) is false)
            return false;
        
        _notes[friendCode] = note;
        return true;
    }
    
    /// <summary> Deletes a note from the database, if one exists </summary>
    public async Task<bool> DeleteNote(string friendCode)
    {
        if (await databaseInfrastructure.DeleteNote(friendCode).ConfigureAwait(false) is false)
            return false;

        _notes.Remove(friendCode);
        return true;
    }
    
    /// <summary> Load all the notes from the database </summary>
    private async Task<bool> LoadNotes()
    {
        if (await databaseInfrastructure.GetNotes().ConfigureAwait(false) is not { } notes)
            return false;

        _notes = notes;
        return true;
    }
}