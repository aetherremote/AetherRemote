using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using AetherRemoteClient.Infrastructure.Database;

namespace AetherRemoteClient.Services;

public class NotesService(DatabaseInfrastructure database)
{
    private Dictionary<string, string> _notes = [];
    private bool _dirty;
    
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
    
    public async Task<bool> LoadNotes()
    {
        if (await database.GetNotes().ConfigureAwait(false) is not { } notes)
            return false;

        _notes = notes;
        _dirty = true;
        return true;
    }
}