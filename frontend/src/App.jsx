import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { motion } from 'framer-motion';

const API_BASE_URL = 'http://localhost:8080';

// রিয়েল-টাইম কাউন্টডাউন টাইমার কম্পোনেন্ট
function HoldTimer({ expiresAt }) {
  const [timeLeft, setTimeLeft] = useState('');

  useEffect(() => {
    const updateTimer = () => {
      const diff = new Date(expiresAt).getTime() - new Date().getTime();
      if (diff <= 0) {
        setTimeLeft('মেয়াদোত্তীর্ণ');
        return;
      }
      const seconds = Math.floor((diff / 1000) % 60);
      const minutes = Math.floor((diff / 1000 / 60) % 60);
      setTimeLeft(`${minutes}:${seconds < 10 ? '0' : ''}${seconds}`);
    };

    updateTimer();
    const interval = setInterval(updateTimer, 1000);
    return () => clearInterval(interval);
  }, [expiresAt]);

  return <span className="text-amber-500 font-mono font-bold">{timeLeft}</span>;
}

export default function App() {
  const [studentNid, setStudentNid] = useState('');
  const [isRegistered, setIsRegistered] = useState(false);
  const [classes, setClasses] = useState([]);
  const [myHolds, setMyHolds] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [successMsg, setSuccessMsg] = useState('');

  // নতুন ক্লাস তৈরির স্টেট (Admin View এর জন্য)
  const [newClassName, setNewClassName] = useState('');
  const [newTotalSeats, setNewTotalSeats] = useState(120);

  useEffect(() => {
    fetchClasses();
    if (studentNid.length === 17) {
      fetchMyHolds();
    }
    
    const interval = setInterval(() => {
      fetchClasses();
      if (studentNid.length === 17) {
        fetchMyHolds();
      }
    }, 5000);
    return () => clearInterval(interval);
  }, [studentNid]);

  const fetchClasses = async () => {
    try {
      const res = await axios.get(`${API_BASE_URL}/classes`);
      setClasses(res.data);
    } catch (err) {
      console.error("Error fetching classes", err);
    }
  };

  const fetchMyHolds = async () => {
    if (!studentNid || studentNid.length !== 17) return;
    try {
      const res = await axios.get(`${API_BASE_URL}/me/holds`, {
        headers: { 'X-Student-Id': studentNid }
      });
      setMyHolds(res.data);
    } catch (err) {
      console.error("Error fetching my holds", err);
    }
  };

  const handleLogin = (e) => {
    e.preventDefault();
    setError('');
    setSuccessMsg('');

    const nidRegex = /^\d{17}$/;

    if (!nidRegex.test(studentNid)) {
      setError('দয়া করে ঠিক ১৭ অঙ্কের জন্ম নিবন্ধন নম্বর (NID) লিখুন।');
      const nidInput = document.getElementById('student-nid-input');
      if (nidInput) {
        nidInput.scrollIntoView({ behavior: 'smooth', block: 'center' });
        nidInput.focus();
      }
      return;
    }

    setIsRegistered(true);
    setSuccessMsg('স্টুডেন্ট আইডি সফলভাবে সেট করা হয়েছে!এখন আবেদন করতে পারেন।');
    fetchMyHolds();
  };

  const handleHoldSeat = async (classId) => {
    if (!studentNid || studentNid.length !== 17) {
      setError('প্রথমে উপরে আপনার সঠিক ১৭ অঙ্কের জন্ম নিবন্ধন নম্বর দিন');
      const nidInput = document.getElementById('student-nid-input');
      if (nidInput) {
        nidInput.scrollIntoView({ behavior: 'smooth', block: 'center' });
        nidInput.focus();
      }
      return;
    }

    setError('');
    setSuccessMsg('');
    try {
      await axios.post(`${API_BASE_URL}/classes/${classId}/holds`, {}, {
        headers: { 'X-Student-Id': studentNid }
      });
      setSuccessMsg('সফলভাবে সিট হোল্ড করা হয়েছে!');
      fetchClasses();
      fetchMyHolds();
      window.scrollTo({ top: 0, behavior: 'smooth' });
    } catch (err) {
      setError(err.response?.data?.message || 'সিট হোল্ড করতে সমস্যা হয়েছে বা সিট খালি নেই।');
      const element = document.getElementById(`class-card-${classId}`);
      if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'center' });
      }
    }
  };

  const handleConfirmHold = async (holdId) => {
    setError('');
    setSuccessMsg('');
    try {
      await axios.post(`${API_BASE_URL}/holds/${holdId}/confirm`, {}, {
        headers: { 'X-Student-Id': studentNid }
      });
      setSuccessMsg('ভর্তি সফলভাবে নিশ্চিত (Confirmed) হয়েছে!');
      fetchClasses();
      fetchMyHolds();
    } catch (err) {
      setError(err.response?.data?.message || 'কনফার্ম করতে সমস্যা হয়েছে।');
    }
  };

  const handleCancelHold = async (holdId) => {
    setError('');
    setSuccessMsg('');
    try {
      await axios.delete(`${API_BASE_URL}/holds/${holdId}`, {
        headers: { 'X-Student-Id': studentNid }
      });
      setSuccessMsg('আবেদন বাতিল করা হয়েছে।');
      fetchClasses();
      fetchMyHolds();
    } catch (err) {
      setError(err.response?.data?.message || 'বাতিল করতে সমস্যা হয়েছে।');
    }
  };

  const handleCreateClass = async (e) => {
    e.preventDefault();
    try {
      await axios.post(`${API_BASE_URL}/classes`, {
        name: newClassName,
        totalSeats: parseInt(newTotalSeats)
      });
      setNewClassName('');
      setNewTotalSeats(120);
      setSuccessMsg('নতুন ক্লাস সফলভাবে তৈরি হয়েছে!');
      fetchClasses();
    } catch (err) {
      setError('ক্লাস তৈরি করতে সমস্যা হয়েছে।');
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-emerald-950 text-slate-100 p-6">
      <div className="max-w-5xl mx-auto space-y-6">
        
        {/* Header & Student NID Login Bar */}
        <motion.header 
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="bg-slate-900/80 backdrop-blur-md p-6 rounded-2xl shadow-xl border border-slate-700/50 flex flex-col md:flex-row justify-between items-center gap-4"
        >
          <div className="font-extrabold text-xl text-emerald-400 tracking-wide flex items-center gap-2">
            🎓 অনলাইন স্কুল ভর্তি সিস্টেম
          </div>
          
          <form onSubmit={handleLogin} className="flex gap-2 w-full md:w-auto">
            <input
              id="student-nid-input"
              type="text"
              maxLength={17}
              placeholder="১৭ অঙ্কের জন্ম নিবন্ধন নম্বর"
              value={studentNid}
              onChange={(e) => {
                const val = e.target.value.replace(/\D/g, '');
                setStudentNid(val);
              }}
              className="bg-slate-800 border border-slate-700 px-4 py-2.5 rounded-xl text-sm w-full md:w-72 text-slate-100 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-emerald-500"
            />
            <button type="submit" className="bg-emerald-600 text-white px-5 py-2.5 rounded-xl text-sm font-semibold hover:bg-emerald-500 shadow-lg shadow-emerald-900/30 transition-all">
              সেট করুন
            </button>
          </form>
        </motion.header>

        {error && <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="bg-red-500/10 border border-red-500/30 text-red-400 px-4 py-3 rounded-xl text-sm backdrop-blur-sm">{error}</motion.div>}
        {successMsg && <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 px-4 py-3 rounded-xl text-sm backdrop-blur-sm">{successMsg}</motion.div>}

        {/* Admin Statistics Summary Cards */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div className="bg-gradient-to-br from-slate-800 to-slate-900 p-5 rounded-2xl border border-slate-700/60 shadow-lg">
            <p className="text-xs uppercase font-medium text-slate-400 tracking-wider">মোট সিট</p>
            <p className="text-3xl font-extrabold text-slate-100 mt-1">
              {classes.reduce((acc, c) => acc + c.totalSeats, 0)}
            </p>
          </div>
          <div className="bg-gradient-to-br from-emerald-950/60 to-slate-900 p-5 rounded-2xl border border-emerald-500/30 shadow-lg">
            <p className="text-xs uppercase font-medium text-emerald-400 tracking-wider">খালি সিট</p>
            <p className="text-3xl font-extrabold text-emerald-400 mt-1">
              {classes.reduce((acc, c) => acc + c.availableSeats, 0)}
            </p>
          </div>
          <div className="bg-gradient-to-br from-amber-950/60 to-slate-900 p-5 rounded-2xl border border-amber-500/30 shadow-lg">
            <p className="text-xs uppercase font-medium text-amber-400 tracking-wider">হোল্ডে আছে</p>
            <p className="text-3xl font-extrabold text-amber-400 mt-1">
              {classes.reduce((acc, c) => acc + c.heldSeats, 0)}
            </p>
          </div>
          <div className="bg-gradient-to-br from-blue-950/60 to-slate-900 p-5 rounded-2xl border border-blue-500/30 shadow-lg">
            <p className="text-xs uppercase font-medium text-blue-400 tracking-wider">নিশ্চিত ভর্তি</p>
            <p className="text-3xl font-extrabold text-blue-400 mt-1">
              {classes.reduce((acc, c) => acc + c.confirmedSeats, 0)}
            </p>
          </div>
        </div>

        {/* My Applications Section */}
        {studentNid.length === 17 && (
          <motion.section 
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="bg-slate-900/80 backdrop-blur-md p-6 rounded-2xl shadow-xl border border-slate-700/50"
          >
            <h2 className="text-xl font-bold mb-4 text-emerald-300">আমার আবেদনসমূহ <span className="text-xs text-slate-400 font-normal">(ID: {studentNid})</span></h2>
            {myHolds.length === 0 ? (
              <p className="text-slate-400 text-sm">আপনার কোনো চলমান আবেদন নেই।</p>
            ) : (
              <div className="space-y-3">
                {myHolds.map((hold) => (
                  <div key={hold.holdId} className="flex flex-col md:flex-row justify-between items-start md:items-center p-4 bg-slate-800/60 rounded-xl border border-slate-700 gap-4">
                    <div>
                      <p className="font-bold text-lg text-slate-100">{hold.className}</p>
                      <p className="text-sm text-slate-300">অবস্থা: <span className="font-bold text-emerald-400">{hold.status}</span></p>
                      {hold.status === 'HELD' && (
                        <div className="text-xs text-amber-400 mt-1">
                          মেয়াদ শেষ হতে বাকি: <HoldTimer expiresAt={hold.expiresAt} />
                        </div>
                      )}
                    </div>
                    <div className="flex gap-2">
                      {hold.status === 'HELD' && (
                        <>
                          <button
                            onClick={() => handleConfirmHold(hold.holdId)}
                            className="bg-emerald-600 text-white px-4 py-2 rounded-xl text-sm font-semibold hover:bg-emerald-500 shadow-md shadow-emerald-900/30 transition-all"
                          >
                            ভর্তি ফি দিন
                          </button>
                          <button
                            onClick={() => handleCancelHold(hold.holdId)}
                            className="bg-red-500/20 border border-red-500/40 text-red-300 px-4 py-2 rounded-xl text-sm font-semibold hover:bg-red-500 hover:text-white transition-all"
                          >
                            বাতিল
                          </button>
                        </>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </motion.section>
        )}

        {/* Classes List & Seat Status */}
        <section className="bg-slate-900/80 backdrop-blur-md p-6 rounded-2xl shadow-xl border border-slate-700/50">
          <h2 className="text-xl font-bold mb-4 text-emerald-300">ক্লাস তালিকা ও সিট অবস্থা</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {classes.map((cls) => (
              <motion.div 
                whileHover={{ scale: 1.01 }}
                key={cls.id} 
                id={`class-card-${cls.id}`} 
                className="border border-slate-700/70 p-5 rounded-2xl flex flex-col justify-between gap-4 bg-slate-800/70 shadow-lg"
              >
                <div>
                  <h3 className="text-lg font-bold text-slate-100">{cls.name}</h3>
                  <div className="grid grid-cols-2 gap-2 mt-3 text-sm text-slate-300">
                    <p>মোট সিট: <span className="font-semibold text-slate-100">{cls.totalSeats}</span></p>
                    <p>খালি সিট: <span className="font-semibold text-emerald-400">{cls.availableSeats}</span></p>
                    <p>হোল্ডে আছে: <span className="font-semibold text-amber-400">{cls.heldSeats}</span></p>
                    <p>নিশ্চিত ভর্তি: <span className="font-semibold text-blue-400">{cls.confirmedSeats}</span></p>
                  </div>
                </div>
                <button
                  onClick={() => handleHoldSeat(cls.id)}
                  disabled={cls.availableSeats <= 0}
                  className={`w-full py-2.5 rounded-xl text-sm font-semibold text-white shadow-md transition-all ${
                    cls.availableSeats > 0 ? 'bg-emerald-600 hover:bg-emerald-500 shadow-emerald-900/30' : 'bg-slate-700 text-slate-400 cursor-not-allowed'
                  }`}
                >
                  {cls.availableSeats > 0 ? 'আবেদন করুন (Hold Seat)' : 'সিট শেষ'}
                </button>
              </motion.div>
            ))}
          </div>
        </section>

        {/* Admin Section: Create Class */}
        <section className="bg-slate-900/80 backdrop-blur-md p-6 rounded-2xl shadow-xl border border-slate-700/50">
          <h2 className="text-xl font-bold mb-4 text-emerald-300">অ্যাডমিন: নতুন ক্লাস তৈরি করুন</h2>
          <form onSubmit={handleCreateClass} className="flex flex-col md:flex-row gap-4">
            <input
              type="text"
              placeholder="ক্লাসের নাম (যেমন: Class 6 Morning Shift)"
              value={newClassName}
              onChange={(e) => setNewClassName(e.target.value)}
              required
              className="bg-slate-800 border border-slate-700 px-4 py-2.5 rounded-xl text-sm flex-1 text-slate-100 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-emerald-500"
            />
            <input
              type="number"
              placeholder="মোট সিট"
              value={newTotalSeats}
              onChange={(e) => setNewTotalSeats(e.target.value)}
              required
              className="bg-slate-800 border border-slate-700 px-4 py-2.5 rounded-xl text-sm w-32 text-slate-100 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-emerald-500"
            />
            <button type="submit" className="bg-indigo-600 text-white px-6 py-2.5 rounded-xl text-sm font-semibold hover:bg-indigo-500 shadow-lg shadow-indigo-900/30 transition-all">
              ক্লাস তৈরি করুন
            </button>
          </form>
        </section>

      </div>
    </div>
  );
}