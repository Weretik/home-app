// Every mechanical rule of docs/rules.md, in the order the rules table lists them. `check-structure.mjs` runs them all;
// `rules.test.mjs` proves each one catches a breach.
import { neverIndexed } from './app-rules.mjs';
import {
  fileNames,
  hookFileNames,
  lineLimits,
  logicHasTests,
  noLiteralColours,
  oneComponentPerFile,
  oneLocale,
} from './file-rules.mjs';
import {
  folderShape,
  noDumpNames,
  srcRootHoldsOnlyTheAreas,
} from './folder-rules.mjs';
import {
  apiClientOnlyFromDataAccess,
  apiClientThroughItsIndex,
  clientInstanceOnlyFromDataAccess,
  featuresAreIsolated,
  importDirection,
} from './import-rules.mjs';

/** Rules over the source tree (`readTree`). */
export const treeRules = [
  srcRootHoldsOnlyTheAreas,
  folderShape,
  noDumpNames,
  lineLimits,
  fileNames,
  oneComponentPerFile,
  hookFileNames,
  featuresAreIsolated,
  importDirection,
  apiClientOnlyFromDataAccess,
  clientInstanceOnlyFromDataAccess,
  apiClientThroughItsIndex,
  noLiteralColours,
  oneLocale,
  logicHasTests,
];

/** Rules over the web root's other files. */
export const rootRules = [neverIndexed];
