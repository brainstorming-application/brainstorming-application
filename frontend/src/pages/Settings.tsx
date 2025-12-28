import { useState } from 'react';
import { useAuthStore } from '../store/authStore';
import { UserRole } from '../types';

export default function Settings() {
  const { user, logout } = useAuthStore();
  const [activeTab, setActiveTab] = useState<'profile' | 'security'>('profile');

  if (!user) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
      </div>
    );
  }

  const getRoleColor = (role: UserRole) => {
    switch (role) {
      case UserRole.EventManager:
        return 'bg-purple-100 text-purple-700 border-purple-200';
      case UserRole.TeamLeader:
        return 'bg-blue-100 text-blue-700 border-blue-200';
      case UserRole.TeamMember:
        return 'bg-green-100 text-green-700 border-green-200';
      default:
        return 'bg-gray-100 text-gray-700 border-gray-200';
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Settings</h1>
        <p className="text-gray-500 mt-1">Manage your account settings and preferences</p>
      </div>

      {/* Tabs */}
      <div className="border-b border-gray-200">
        <nav className="-mb-px flex space-x-8">
          <button
            onClick={() => setActiveTab('profile')}
            className={`py-4 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'profile'
                ? 'border-indigo-500 text-indigo-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Profile
          </button>
          <button
            onClick={() => setActiveTab('security')}
            className={`py-4 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'security'
                ? 'border-indigo-500 text-indigo-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Security
          </button>
        </nav>
      </div>

      {/* Profile Tab */}
      {activeTab === 'profile' && (
        <div className="bg-white rounded-xl shadow-sm p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-6">Profile Information</h2>

          <div className="space-y-6">
            {/* User Avatar/Initials */}
            <div className="flex items-center space-x-4">
              <div className="w-20 h-20 bg-gradient-to-br from-indigo-400 to-purple-500 rounded-full flex items-center justify-center text-white text-2xl font-bold">
                {user.firstName?.charAt(0)}{user.lastName?.charAt(0)}
              </div>
              <div>
                <h3 className="text-xl font-semibold text-gray-900">
                  {user.firstName} {user.lastName}
                </h3>
                <p className="text-sm text-gray-500">{user.email}</p>
              </div>
            </div>

            {/* User ID */}
            <div className="border-t border-gray-200 pt-6">
              <label className="block text-sm font-medium text-gray-700 mb-2">User ID</label>
              <div className="flex items-center space-x-3">
                <code className="px-4 py-2 bg-gray-50 border border-gray-200 rounded-lg text-sm font-mono text-gray-900 flex-1">
                  {user.id}
                </code>
                <button
                  onClick={() => {
                    navigator.clipboard.writeText(user.id);
                    alert('User ID copied to clipboard!');
                  }}
                  className="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors text-sm font-medium"
                >
                  Copy
                </button>
              </div>
              <p className="mt-2 text-xs text-gray-500">Your unique user identifier</p>
            </div>

            {/* Personal Information */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 border-t border-gray-200 pt-6">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">First Name</label>
                <div className="px-4 py-2 bg-gray-50 border border-gray-200 rounded-lg text-gray-900">
                  {user.firstName}
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Last Name</label>
                <div className="px-4 py-2 bg-gray-50 border border-gray-200 rounded-lg text-gray-900">
                  {user.lastName}
                </div>
              </div>

              <div className="md:col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-2">Email Address</label>
                <div className="px-4 py-2 bg-gray-50 border border-gray-200 rounded-lg text-gray-900">
                  {user.email}
                </div>
              </div>

              <div className="md:col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-2">Role</label>
                <div className={`inline-flex items-center px-4 py-2 rounded-lg border ${getRoleColor(user.role)}`}>
                  <span className="font-medium">{user.role}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Security Tab */}
      {activeTab === 'security' && (
        <div className="bg-white rounded-xl shadow-sm p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-6">Security Settings</h2>

          <div className="space-y-6">
            {/* Password Change */}
            <div className="border-b border-gray-200 pb-6">
              <h3 className="text-md font-medium text-gray-900 mb-2">Change Password</h3>
              <p className="text-sm text-gray-500 mb-4">
                Update your password to keep your account secure
              </p>
              <button
                disabled
                className="px-4 py-2 bg-gray-100 text-gray-400 rounded-lg cursor-not-allowed text-sm font-medium"
              >
                Change Password (Coming Soon)
              </button>
            </div>

            {/* Session Management */}
            <div className="border-b border-gray-200 pb-6">
              <h3 className="text-md font-medium text-gray-900 mb-2">Active Sessions</h3>
              <p className="text-sm text-gray-500 mb-4">
                Manage your active sessions across different devices
              </p>
              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm font-medium text-blue-900">Current Session</p>
                    <p className="text-xs text-blue-700 mt-1">
                      {new Date().toLocaleString()} • {navigator.userAgent.split(' ')[0]}
                    </p>
                  </div>
                  <span className="px-2 py-1 bg-blue-100 text-blue-700 rounded text-xs font-medium">
                    Active
                  </span>
                </div>
              </div>
            </div>

            {/* Account Actions */}
            <div>
              <h3 className="text-md font-medium text-gray-900 mb-2">Account Actions</h3>
              <p className="text-sm text-gray-500 mb-4">
                Manage your account and data
              </p>
              <div className="space-y-3">
                <button
                  onClick={() => {
                    if (confirm('Are you sure you want to logout?')) {
                      logout();
                      window.location.href = '/login';
                    }
                  }}
                  className="w-full md:w-auto px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors text-sm font-medium"
                >
                  Logout
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Account Statistics */}
      <div className="bg-white rounded-xl shadow-sm p-6">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Account Information</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div className="p-4 bg-indigo-50 rounded-lg">
            <p className="text-sm text-indigo-600 font-medium">User ID</p>
            <p className="text-xs text-indigo-500 mt-1 font-mono truncate">{user.id}</p>
          </div>
          <div className="p-4 bg-purple-50 rounded-lg">
            <p className="text-sm text-purple-600 font-medium">Role</p>
            <p className="text-lg text-purple-900 mt-1 font-semibold">{user.role}</p>
          </div>
          <div className="p-4 bg-blue-50 rounded-lg">
            <p className="text-sm text-blue-600 font-medium">Email</p>
            <p className="text-xs text-blue-500 mt-1 truncate">{user.email}</p>
          </div>
        </div>
      </div>
    </div>
  );
}

