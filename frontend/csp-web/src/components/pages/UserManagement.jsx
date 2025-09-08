import { useState, useEffect } from 'react';
import MemberManagement from '../MemberManagement';
import './UserManagement.css';

const UserManagement = ({ user }) => {
  if (user.role !== 'Administrator') {
    return (
      <div className="access-denied">
        <div className="access-denied-icon">🚫</div>
        <h2>Access Denied</h2>
        <p>You don't have permission to access user management.</p>
      </div>
    );
  }

  return (
    <MemberManagement user={user} />
  );
};

export default UserManagement;
