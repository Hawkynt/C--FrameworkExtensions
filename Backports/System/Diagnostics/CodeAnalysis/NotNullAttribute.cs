x���N�0 9�+|/���	�r��|��^7Q��r�B��D��9�4��u�E[��1��f�5&;��� ���H�"��~Twj|� g!�T�B�DJh-�M��f��IAQ�ik����߻�r�e����a�e�?�¯�0F�����_���MuЕIzc]�<�y�s_�7�6�.��?�V���9�/E���R�                                                                                                                                                                                                                                                                                                                                 in the hope that
//     it will be useful, but WITHOUT ANY WARRANTY; without even the implied
//     warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
//     the GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with Hawkynt's .NET Framework extensions.
//     If not, see <http://www.gnu.org/licenses/>.
// */
// #endregion

namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class NotNullAttribute : Attribute { }

#endif