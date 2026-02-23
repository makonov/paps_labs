import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { getTalks } from '../api';

interface Talk {
  id: number;
  title: string;
  speakerId: number;
  startTime: string;
  room: string;
}

interface TalksProps {
  token: string;
}

const Talks: React.FC<TalksProps> = ({ token }) => {
  const { id } = useParams<{ id: string }>();
  const conferenceId = Number(id);
  const [talks, setTalks] = useState<Talk[]>([]);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!conferenceId) return;
    const fetch = async () => {
      try {
        const data = await getTalks(token, conferenceId);
        setTalks(data);
      } catch {
        setError('Ошибка при загрузке докладов');
      }
    };
    fetch();
  }, [token, conferenceId]);

  return (
    <div>
      <h2>Доклады</h2>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <ul>
        {talks.map(t => (
          <li key={t.id}>
            {t.title} — {t.startTime} — Комната: {t.room}
          </li>
        ))}
      </ul>
    </div>
  );
};

export default Talks;