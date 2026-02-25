import React, { useEffect, useState } from 'react';
import type { Conference } from '../api';
import {
  getConferences,
  createConference,
  deleteConference,
  updateConference,
} from '../api';
import { Link } from 'react-router-dom';

interface Props {
  token: string | null;
  role: string | null;
}

const Conferences: React.FC<Props> = ({ token, role }) => {
  const [conferences, setConferences] = useState<Conference[]>([]);
  const [error, setError] = useState<string | null>(null);

  const [name, setName] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [location, setLocation] = useState('');

  const [editingId, setEditingId] = useState<number | null>(null);
  const [editName, setEditName] = useState('');
  const [editStartDate, setEditStartDate] = useState('');
  const [editEndDate, setEditEndDate] = useState('');
  const [editLocation, setEditLocation] = useState('');

  useEffect(() => { load(); }, []);

  const load = async () => {
    try {
      const data = await getConferences();
      setConferences(data);
      setError(null);
    } catch (err: any) {
      setError(err.message);
    }
  };

  const handleCreate = async () => {
    if (role !== 'organizer') return setError('Нужна роль organizer');
    try {
      await createConference({ name, startDate, endDate, location }, token!);
      setName(''); setStartDate(''); setEndDate(''); setLocation('');
      setError(null);
      load();
    } catch (err: any) { setError(err.message); }
  };

  const handleDelete = async (id: number) => {
    if (role !== 'organizer') return setError('Нужна роль organizer');
    try { await deleteConference(id, token!); setError(null); load(); } 
    catch (err: any) { setError(err.message); }
  };

  const startEdit = (c: Conference) => {
    setEditingId(c.id!);
    setEditName(c.name);
    setEditStartDate(c.startDate);
    setEditEndDate(c.endDate);
    setEditLocation(c.location);
  };

  const handleEditSave = async () => {
    if (role !== 'organizer' || editingId === null) return;
    try {
      await updateConference(editingId, { name: editName, startDate: editStartDate, endDate: editEndDate, location: editLocation }, token!);
      setEditingId(null); setError(null); load();
    } catch (err: any) { setError(err.message); }
  };

  const handleEditCancel = () => setEditingId(null);

  return (
    <div>
      <h2>Конференции</h2>
      {error && <div style={{ color: 'red' }}>{error}</div>}

      <ul>
        {conferences.map(c => (
          <li key={c.id} style={{ marginBottom: '10px' }}>
            {editingId === c.id ? (
              <div>
                <input value={editName} onChange={e => setEditName(e.target.value)} />
                <input value={editStartDate} onChange={e => setEditStartDate(e.target.value)} />
                <input value={editEndDate} onChange={e => setEditEndDate(e.target.value)} />
                <input value={editLocation} onChange={e => setEditLocation(e.target.value)} />
                <button onClick={handleEditSave}>Сохранить</button>
                <button onClick={handleEditCancel}>Отмена</button>
              </div>
            ) : (
              <div>
                <strong>{c.name}</strong> — {c.location} — {c.startDate} / {c.endDate}
                <Link to={`/conferences/${c.id}/talks`} style={{ marginLeft: '10px' }}>Доклады</Link>

                {role === 'organizer' && (
                  <>
                    <button onClick={() => startEdit(c)} style={{ marginLeft: '10px' }}>Редактировать</button>
                    <button onClick={() => handleDelete(c.id!)} style={{ marginLeft: '5px' }}>Удалить</button>
                  </>
                )}
              </div>
            )}
          </li>
        ))}
      </ul>

      {role === 'organizer' && editingId === null && (
        <div>
          <h3>Создать конференцию</h3>
          <input placeholder="Название" value={name} onChange={e => setName(e.target.value)} />
          <input placeholder="StartDate YYYY-MM-DD" value={startDate} onChange={e => setStartDate(e.target.value)} />
          <input placeholder="EndDate YYYY-MM-DD" value={endDate} onChange={e => setEndDate(e.target.value)} />
          <input placeholder="Локация" value={location} onChange={e => setLocation(e.target.value)} />
          <button onClick={handleCreate}>Создать</button>
        </div>
      )}
    </div>
  );
};

export default Conferences;