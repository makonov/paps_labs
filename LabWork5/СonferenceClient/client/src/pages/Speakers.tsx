import React, { useEffect, useState } from 'react';
import type { Speaker } from '../api';
import { getSpeakers, createSpeaker, updateSpeaker, deleteSpeaker } from '../api';

interface Props { token: string | null; role: string | null; }

const Speakers: React.FC<Props> = ({ token, role }) => {
  const [speakers, setSpeakers] = useState<Speaker[]>([]);
  const [error, setError] = useState<string | null>(null);

  const [name, setName] = useState('');
  const [bio, setBio] = useState('');

  const [editingId, setEditingId] = useState<number | null>(null);
  const [editName, setEditName] = useState('');
  const [editBio, setEditBio] = useState('');

  useEffect(() => { load(); }, []);

  const load = async () => {
    try { const data = await getSpeakers(); setSpeakers(data); setError(null); } 
    catch (err: any) { setError(err.message); }
  };

  const handleCreate = async () => {
    if (role !== 'organizer') return setError('Нужна роль organizer');
    try { await createSpeaker({ name, bio }, token!); setName(''); setBio(''); load(); setError(null); } 
    catch (err: any) { setError(err.message); }
  };

  const startEdit = (s: Speaker) => { setEditingId(s.id!); setEditName(s.name); setEditBio(s.bio || ''); };
  const handleEditSave = async () => {
    if (role !== 'organizer' || editingId === null) return;
    try { await updateSpeaker(editingId, { name: editName, bio: editBio }, token!); setEditingId(null); load(); setError(null); } 
    catch (err: any) { setError(err.message); }
  };
  const handleEditCancel = () => setEditingId(null);
  const handleDelete = async (id: number) => {
    if (role !== 'organizer') return;
    try { await deleteSpeaker(id, token!); setError(null); load(); } 
    catch (err: any) { setError(err.message); }
  };

  return (
    <div>
      <h2>Спикеры</h2>
      {error && <div style={{ color: 'red' }}>{error}</div>}
      <ul>
        {speakers.map(s => (
          <li key={s.id} style={{ marginBottom: '10px' }}>
            {editingId === s.id ? (
              <div>
                <input value={editName} onChange={e => setEditName(e.target.value)} />
                <input value={editBio} onChange={e => setEditBio(e.target.value)} />
                <button onClick={handleEditSave}>Сохранить</button>
                <button onClick={handleEditCancel}>Отмена</button>
              </div>
            ) : (
              <div>
                <strong>{s.name}</strong> — {s.bio || 'Биография отсутствует'}
                {role === 'organizer' && (
                  <>
                    <button onClick={() => startEdit(s)} style={{ marginLeft: '10px' }}>Редактировать</button>
                    <button onClick={() => handleDelete(s.id!)} style={{ marginLeft: '5px' }}>Удалить</button>
                  </>
                )}
              </div>
            )}
          </li>
        ))}
      </ul>

      {role === 'organizer' && editingId === null && (
        <div>
          <h3>Создать спикера</h3>
          <input placeholder="Имя" value={name} onChange={e => setName(e.target.value)} />
          <input placeholder="Bio" value={bio} onChange={e => setBio(e.target.value)} />
          <button onClick={handleCreate}>Создать</button>
        </div>
      )}
    </div>
  );
};

export default Speakers;