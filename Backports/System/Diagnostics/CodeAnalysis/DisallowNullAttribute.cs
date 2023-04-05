ref: refs/remotes/origin/master
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                buted in the hope that
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

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false)]
#if COMPILE_TO_EXTENSION_DLL
public
#else
  internal
#endif
  sealed class DisallowNullAttribute : Attribute { }

#endif