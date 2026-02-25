import React, { useEffect, useState } from 'react';
import type { Talk, Speaker } from '../api';
import { getTalks, createTalk, updateTalk, deleteTalk, getSpeakers } from '../api';
import { useParams } from 'react-router-dom';

interface Props { token: string | null; role: string | null; }

const Talks: React.FC<Props> = ({ token, role }) => {
  const { id } = useParams();
  const conferenceId = Number(id);

  const [talks, setTalks] = useState<Talk[]>([]);
  const [speakers, setSpeakers] = useState<Speaker[]>([]);
  const [error, setError] = useState<string | null>(null);

  const [title, setTitle] = useState('');
  const [speakerId, setSpeakerId] = useState<number | null>(null);
  const [startTime, setStartTime] = useState('');
  const [room, setRoom] = useState('');

  const [editingId, setEditingId] = useState<number | null>(null);
  const [editTitle, setEditTitle] = useState('');
  const [editSpeakerId, setEditSpeakerId] = useState<number | null>(null);
  const [editStartTime, setEditStartTime] = useState('');
  const [editRoom, setEditRoom] = useState('');

  useEffect(() => { load(); }, [id]);

  const load = async () => {
    try {
      const [talksData, speakersData] = await Promise.all([getTalks(conferenceId), getSpeakers()]);
      setTalks(talksData); setSpeakers(speakersData); setError(null);
    } catch (err: any) { setError(err.message); }
  };

  const handleCreate = async () => {
    if (role !== 'speaker') return setError('Нужна роль speaker');
    if (!speakerId) return setError('Выберите спикера');
    try {
      await createTalk(conferenceId, { title, speakerId, startTime, room }, token!);
      setTitle(''); setSpeakerId(null); setStartTime(''); setRoom(''); setError(null); load();
    } catch (err: any) { setError(err.message); }
  };

  const startEdit = (t: Talk) => { setEditingId(t.id!); setEditTitle(t.title); setEditSpeakerId(t.speakerId); setEditStartTime(t.startTime); setEditRoom(t.room); };
  const handleEditSave = async () => {
    if (role !== 'speaker' || editingId === null || !editSpeakerId) return;
    try { await updateTalk(conferenceId, editingId, { title: editTitle, speakerId: editSpeakerId, startTime: editStartTime, room: editRoom }, token!); setEditingId(null); setError(null); load(); } 
    catch (err: any) { setError(err.message); }
  };
  const handleDelete = async (talkId: number) => {
    if (role !== 'speaker') return;
    try { await deleteTalk(conferenceId, talkId, token!); setError(null); load(); } 
    catch (err: any) { setError(err.message); }

  };

  const getSpeakerName = (id: number) => speakers.find(s => s.id === id)?.name || `ID ${id}`;

  return (
    <div>
      <h2>Доклады</h2>
      {error && <div style={{ color: 'red' }}>{error}</div>}

      <ul>
        {talks.map(t => (
          <li key={t.id} style={{ marginBottom: '10px' }}>
            {editingId === t.id ? (
              <div>
                <input value={editTitle} onChange={e => setEditTitle(e.target.value)} />
                <select value={editSpeakerId ?? ''} onChange={e => setEditSpeakerId(Number(e.target.value))}>
                  <option value="">Выберите спикера</option>
                  {speakers.map(s => (<option key={s.id} value={s.id}>{s.name}</option>))}
                </select>
                <input value={editStartTime} onChange={e => setEditStartTime(e.target.value)} />
                <input value={editRoom} onChange={e => setEditRoom(e.target.value)} />
                <button onClick={handleEditSave}>Сохранить</button>
                <button onClick={() => setEditingId(null)}>Отмена</button>
              </div>
            ) : (
              <div>
                <strong>{t.title}</strong> — Спикер: {getSpeakerName(t.speakerId)} — {t.room} — {t.startTime}
                {role === 'speaker' && (
                  <>
                    <button onClick={() => startEdit(t)} style={{ marginLeft: '10px' }}>Редактировать</button>
                    <button onClick={() => handleDelete(t.id!)} style={{ marginLeft: '5px' }}>Удалить</button>
                  </>
                )}
              </div>
            )}
          </li>
        ))}
      </ul>

      {role === 'speaker' && editingId === null && (
        <div>
          <h3>Создать доклад</h3>
          <input placeholder="Название" value={title} onChange={e => setTitle(e.target.value)} />
          <select value={speakerId ?? ''} onChange={e => setSpeakerId(Number(e.target.value))}>
            <option value="">Выберите спикера</option>
            {speakers.map(s => (<option key={s.id} value={s.id}>{s.name}</option>))}
          </select>
          <input placeholder="StartTime YYYY-MM-DDTHH:MM" value={startTime} onChange={e => setStartTime(e.target.value)} />
          <input placeholder="Room" value={room} onChange={e => setRoom(e.target.value)} />
          <button onClick={handleCreate}>Создать</button>
        </div>
      )}
    </div>
  );
};

export default Talks;