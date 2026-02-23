import React, { useEffect, useState } from 'react';
import { getSpeakers } from '../api';

interface Speaker {
  id: number;
  name: string;
  bio: string;
}

interface SpeakersProps {
  token: string;
}

const Speakers: React.FC<SpeakersProps> = () => {
  const [speakers, setSpeakers] = useState<Speaker[]>([]);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetch = async () => {
      try {
        const data = await getSpeakers();
        setSpeakers(data);
      } catch {
        setError('Ошибка при загрузке спикеров');
      }
    };
    fetch();
  }, []);

  return (
    <div>
      <h2>Спикеры</h2>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <ul>
        {speakers.map(s => (
          <li key={s.id}>
            {s.name} — {s.bio}
          </li>
        ))}
      </ul>
    </div>
  );
};

export default Speakers;